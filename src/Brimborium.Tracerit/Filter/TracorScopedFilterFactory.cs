#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace Brimborium.Tracerit.Filter;

/// <summary>
/// Produces instances of <see cref="ITracorScopedFilter"/> classes based on the given providers.
/// </summary>
[DebuggerDisplay("{DebuggerToString(),nq}")]
[DebuggerTypeProxy(typeof(TracorFactoryDebugView))]
public class TracorScopedFilterFactory : ITracorScopedFilterFactory {
    private readonly ConcurrentDictionary<string, TracorScopedFilter> _DictFilterByName = new(StringComparer.Ordinal);
    private readonly List<string> _ListSourceNameFromRegistration = [];
    private readonly List<string> _ListSourceNameFromOptions = [];
    private string[] _ListSourceName = [];
    private readonly Lock _Lock = new();
    private bool _Disposed;
    private IDisposable? _FactoryChangeTokenRegistration;
    private IDisposable? _FiltersChangeTokenRegistration;
    private TracorScopedFilterOptionsBySourceName _TracorScopedFilterOptions = new(LogLevel.Trace, [], []);

    public TracorScopedFilterFactory(
        ) : this(
        [],
            new StaticTracorScopedFilterOptionsMonitor(new TracorScopedFilterOptions()),
            new StaticTracorScopedFilterFactoryOptionsMonitor(new TracorScopedFilterFactoryOptions())
        ) {
    }

    public TracorScopedFilterFactory(
        IEnumerable<ITracorScopedFilterSource> listScopedTracorSource
        ) : this(
            listScopedTracorSource,
            new StaticTracorScopedFilterOptionsMonitor(new TracorScopedFilterOptions()),
            new StaticTracorScopedFilterFactoryOptionsMonitor(new TracorScopedFilterFactoryOptions())
            ) {
    }

    public TracorScopedFilterFactory(
        IEnumerable<ITracorScopedFilterSource> listScopedTracorSource,
        TracorScopedFilterOptions filterOptions,
        TracorScopedFilterFactoryOptions factoryOptions
        ) : this(
            listScopedTracorSource,
            new StaticTracorScopedFilterOptionsMonitor(filterOptions),
            new StaticTracorScopedFilterFactoryOptionsMonitor(factoryOptions)) {
    }

    public TracorScopedFilterFactory(
        IEnumerable<ITracorScopedFilterSource> listScopedTracorSource,
        IOptionsMonitor<TracorScopedFilterOptions> filterOption,
        IOptionsMonitor<TracorScopedFilterFactoryOptions> factoryOptions
        ) {
        foreach (var scopedTracorSource in listScopedTracorSource.OrderBy(i => i.GetSourceName())) {
            this.AddTracorScopedFilterSource(scopedTracorSource, dispose: false);
        }

        this._FactoryChangeTokenRegistration = factoryOptions.OnChange(this.RefreshFactoryOption);
        this.InitFactoryOption(factoryOptions.CurrentValue);

        this._FiltersChangeTokenRegistration = filterOption.OnChange(this.RefreshFilters);
        this.InitFilters(filterOption.CurrentValue);
    }

    /// <summary>
    /// Creates new instance of <see cref="ITracorScopedFilterFactory"/> configured using provided <paramref name="configure"/> delegate.
    /// </summary>
    /// <param name="configure">A delegate to configure the <see cref="ITracorScopedFilterBuilder"/>.</param>
    /// <returns>The <see cref="ITracorScopedFilterFactory"/> that was created.</returns>
    public static ITracorScopedFilterFactory Create(Action<ITracorScopedFilterBuilder> configure) {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTracorScopedFilter(configure);
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        ITracorScopedFilterFactory tracorFactory = serviceProvider.GetRequiredService<ITracorScopedFilterFactory>();
        return new DisposingTracorScopedFilterFactory(tracorFactory, serviceProvider);
    }

    private void RefreshFactoryOption(TracorScopedFilterFactoryOptions options) {
        if (this._ListSourceNameFromOptions.SequenceEqual(options.ListSourceName)) {
            // nothing changed
            return;
        }

        using (this._Lock.EnterScope()) {
            {
                this._ListSourceNameFromOptions.Clear();
                this._ListSourceNameFromOptions.AddRange(options.ListSourceName);
                this._ListSourceName = this.CreateListSourceName();
            }

            var tracorScopedFilterOptions
                = TracorScopedFilterOptionsBySourceName.Create(
                    this._TracorScopedFilterOptions.MinLevel,
                    this._TracorScopedFilterOptions.Rules,
                    this._ListSourceName);
            this._TracorScopedFilterOptions = tracorScopedFilterOptions;

            {
                foreach (var (categoryName, tracorScopedFilter) in this._DictFilterByName) {
                    tracorScopedFilter.ListCategoryInformation = this.CreateCategoryInformation(categoryName);
                    tracorScopedFilter.ListFilteredTracors = this.ApplyFiltersWithinLock(tracorScopedFilter.ListCategoryInformation, tracorScopedFilterOptions);
                }
            }

            this.UpdateIncludingAllSubScopeWithinLock();
        }
    }

    private void InitFactoryOption(TracorScopedFilterFactoryOptions options) {
        this._ListSourceNameFromOptions.Clear();
        this._ListSourceNameFromOptions.AddRange(options.ListSourceName);
        this._ListSourceName = this.CreateListSourceName();
    }

    [MemberNotNull(nameof(_TracorScopedFilterOptions))]
    private void InitFilters(TracorScopedFilterOptions filterOptions) {
        using (this._Lock.EnterScope()) {
            var tracorScopedFilterOptions
                = TracorScopedFilterOptionsBySourceName.Create(
                    filterOptions.MinLevel,
                    filterOptions.RulesInternal,
                    this._ListSourceName);
            this._TracorScopedFilterOptions = tracorScopedFilterOptions;
            {
                var listRule = filterOptions.RulesInternal;
                foreach (var rule in listRule) {
                    if (rule.CategoryName is { Length: > 0 } categoryName) {
                        // ensure a TracorScopedFilter exists for this categoryName
                        if (this._DictFilterByName.ContainsKey(categoryName)) {
                            // nothing to do
                        } else {
                            // create a new one
                            var tracorScopedFilter = new TracorScopedFilter(categoryName, []);
                            this._DictFilterByName[categoryName] = tracorScopedFilter;
                            tracorScopedFilter.ListCategoryInformation = this.CreateCategoryInformation(categoryName);
                            tracorScopedFilter.ListFilteredTracors = this.ApplyFiltersWithinLock(tracorScopedFilter.ListCategoryInformation, tracorScopedFilterOptions);
                        }
                    }
                }
                this.UpdateIncludingAllSubScopeWithinLock();
            }
        }
    }

    private void UpdateIncludingAllSubScopeWithinLock() {
        var listFilterScope = this._DictFilterByName.Keys
            .OrderBy(kvp => kvp)
            .ToArray();
        foreach (var (scope, filter) in this._DictFilterByName) {
            var includingAllSubScope = true;
            if (1 < listFilterScope.Length) {
                int scopeLength = scope.Length;
                int index = 0;
                if (2 < listFilterScope.Length) {
                    var pos = Array.BinarySearch(listFilterScope, scope);
                    index = (pos < 0) ? ~pos : pos;
                } else {
                    index = 0;
                }
                bool canBreak = false;
                for (; index < listFilterScope.Length; index++) {
                    string? filterScope = listFilterScope[index];
                    if (filterScope.StartsWith(scope)) {
                        canBreak = true;
                        if (scopeLength < filterScope.Length) {
                            if ('.' == filterScope[scopeLength]) {
                                includingAllSubScope = false;
                                break;
                            }
                        }
                    } else {
                        if (canBreak) {
                            break;
                        }
                    }
                }
            }
            filter.SetIncludingAllSubScope(includingAllSubScope);
        }
    }

    [MemberNotNull(nameof(_TracorScopedFilterOptions))]
    private void RefreshFilters(TracorScopedFilterOptions filterOptions) {
        using (this._Lock.EnterScope()) {
            var tracorScopedFilterOptions
                = TracorScopedFilterOptionsBySourceName.Create(
                    filterOptions.MinLevel,
                    filterOptions.RulesInternal,
                    this._ListSourceName);
            this._TracorScopedFilterOptions = tracorScopedFilterOptions;
            {
                foreach (var (categoryName, tracorScopedFilter) in this._DictFilterByName) {
                    tracorScopedFilter.ListCategoryInformation = this.CreateCategoryInformation(categoryName);
                    tracorScopedFilter.ListFilteredTracors = this.ApplyFiltersWithinLock(tracorScopedFilter.ListCategoryInformation, tracorScopedFilterOptions);
                }
            }

            this.UpdateIncludingAllSubScopeWithinLock();
        }
    }

    /// <summary>
    /// Creates an <see cref="ITracorScopedFilter"/> with the given <paramref name="categoryName"/>.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the tracor.</param>
    /// <returns>The <see cref="ITracorScopedFilter"/> that was created.</returns>
    public ITracorScopedFilter CreateTracorScopedFilter(string categoryName) {
        if (this.CheckDisposed()) {
            throw new ObjectDisposedException(nameof(TracorScopedFilterFactory));
        }

        if (!this._DictFilterByName.TryGetValue(categoryName, out TracorScopedFilter? tracorScopedFilter)) {
            using (this._Lock.EnterScope()) {
                if (!this._DictFilterByName.TryGetValue(categoryName, out tracorScopedFilter)) {
                    tracorScopedFilter = new TracorScopedFilter(categoryName, []);
                    this._DictFilterByName[categoryName] = tracorScopedFilter;
                    var listTracorInformation = this.CreateCategoryInformation(categoryName);
                    tracorScopedFilter.ListCategoryInformation = listTracorInformation;
                    tracorScopedFilter.ListFilteredTracors = this.ApplyFiltersWithinLock(listTracorInformation, this._TracorScopedFilterOptions);

                    this.UpdateIncludingAllSubScopeWithinLock();
                }
            }
        }

        return tracorScopedFilter;
    }

    /// <summary>
    /// Adds the given provider to those used in creating <see cref="ITracorScopedFilter"/> instances.
    /// </summary>
    /// <param name="tsfSourceName">The <see cref="ITracorScopedFilterSource"/> to add.</param>
    public void AddTracorScopedFilterSourceRegister(ITracorScopedFilterSource tsfSourceName) {
        if (this.CheckDisposed()) {
            throw new ObjectDisposedException(nameof(TracorScopedFilterFactory));
        }

        ArgumentNullException.ThrowIfNull(tsfSourceName);

        using (this._Lock.EnterScope()) {
            this.AddTracorScopedFilterSource(tsfSourceName, dispose: true);

            var tracorScopedFilterOptions
                = TracorScopedFilterOptionsBySourceName.Create(
                    this._TracorScopedFilterOptions.MinLevel,
                    this._TracorScopedFilterOptions.Rules,
                    this._ListSourceName);
            this._TracorScopedFilterOptions = tracorScopedFilterOptions;

            var sourceName = tsfSourceName.GetSourceName();
            foreach (KeyValuePair<string, TracorScopedFilter> existingTracor in this._DictFilterByName) {
                string category = existingTracor.Key;
                TracorScopedFilter tracorScopedFilter = existingTracor.Value;

                var listCategoryInformation = AddCategory(sourceName, category, tracorScopedFilter);
                tracorScopedFilter.ListCategoryInformation = listCategoryInformation;
                tracorScopedFilter.ListFilteredTracors = this.ApplyFiltersWithinLock(tracorScopedFilter.ListCategoryInformation, tracorScopedFilterOptions);
            }

            this.UpdateIncludingAllSubScopeWithinLock();
        }

        TracorScopedFilterCategoryInformation[] AddCategory(
            string sourceName,
            string category,
            TracorScopedFilter tracorScopedFilter) {
            TracorScopedFilterCategoryInformation[] listCategoryInformation = tracorScopedFilter.ListCategoryInformation;

            int length = listCategoryInformation.Length;
            int newTracorIndex;
            var item = new TracorScopedFilterCategoryInformation(
                    sourceName: sourceName,
                    category: category);
            if (length == 0) {
                newTracorIndex = length;

                Array.Resize(ref listCategoryInformation, length + 1);
                listCategoryInformation[newTracorIndex] = item;
            } else {
                newTracorIndex = length;
                for (int idx = 0; idx < length; idx++) {
                    var cmp = StringComparer.OrdinalIgnoreCase.Compare(
                        listCategoryInformation[idx].SourceName,
                        sourceName
                        );
                    if (cmp > 0) {
                        newTracorIndex = idx;
                        break;
                    }
                }
                Array.Resize(ref listCategoryInformation, length + 1);
                for (int idx = length; idx > newTracorIndex; idx--) {
                    listCategoryInformation[idx] = listCategoryInformation[idx - 1];
                }
                listCategoryInformation[newTracorIndex] = item;
            }
            return listCategoryInformation;
        }

    }
    private void AddTracorScopedFilterSource(ITracorScopedFilterSource tsfSourceName, bool dispose) {
        var sourceName = tsfSourceName.GetSourceName();

        var pos = this._ListSourceNameFromRegistration.BinarySearch(sourceName);
        if (0 <= pos) {
            this._ListSourceNameFromRegistration.Insert(pos, sourceName);
        } else {
            this._ListSourceNameFromRegistration.Insert(~pos, sourceName);
        }

        this._ListSourceName = this.CreateListSourceName();
    }

    private string[] CreateListSourceName() {
        // distinct sourceNames
        int count = this._ListSourceNameFromRegistration.Count + this._ListSourceNameFromOptions.Count;
        HashSet<string> result = new(count, StringComparer.OrdinalIgnoreCase);
        foreach (var name in this._ListSourceNameFromRegistration) { result.Add(name); }
        foreach (var name in this._ListSourceNameFromOptions) { result.Add(name); }

        return result
            .OrderBy(a => a, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private TracorScopedFilterCategoryInformation[] CreateCategoryInformation(string categoryName) {
        string[] listSourceName = this._ListSourceName;
        int length = listSourceName.Length;
        var listCategoryInformation = new List<TracorScopedFilterCategoryInformation>(length);
        for (int index = 0; index < length; index++) {
            var categoryInformation = new TracorScopedFilterCategoryInformation(
                listSourceName[index],
                categoryName);
            listCategoryInformation.Add(categoryInformation);

            // We do not need to check for NullTracor<T>.Instance as no provider would reasonably return it (the <T> handling is at
            // outer tracors level, not inner level tracors in Tracor/TracorProvider).
            //if (tracorInformation.Source != Brimborium.Tracerit.Filter.Internal.NullTracorScopedFilterProvider.Instance) {
            //    tracors.Add(tracorInformation);
            //}
        }
        return listCategoryInformation.ToArray();
    }

    private TracorScopedFilterCategoryFiltered[] ApplyFiltersWithinLock(
        TracorScopedFilterCategoryInformation[] listCategoryInformation,
        TracorScopedFilterOptionsBySourceName tracorScopedFilterOptions) {
        var listCategoryInformationLength = listCategoryInformation.Length;
        List<TracorScopedFilterCategoryFiltered> resultTracors = new(listCategoryInformationLength);

        for (int indexCategoryInformation = 0; indexCategoryInformation < listCategoryInformationLength; indexCategoryInformation++) {
            ref TracorScopedFilterCategoryInformation categoryInformation = ref listCategoryInformation[indexCategoryInformation];
            TracorScopedFilterRuleSelector.Select(
                tracorScopedFilterOptions,
                categoryInformation.SourceName,
                categoryInformation.Category,
                out LogLevel? minLevel,
                out Func<string?, string?, LogLevel, bool>? filter);

            if (minLevel is not null and > LogLevel.Critical) {
                continue;
            }

            resultTracors.Add(
                new TracorScopedFilterCategoryFiltered(
                    categoryInformation.SourceName,
                    categoryInformation.Category,
                    minLevel,
                    filter));
        }

        return resultTracors.ToArray();
    }

    /// <summary>
    /// Check if the factory has been disposed.
    /// </summary>
    /// <returns><see langword="true" /> when <see cref="Dispose()"/> as been called</returns>
    protected virtual bool CheckDisposed() => this._Disposed;

    /// <inheritdoc/>
    public void Dispose() {
        if (!this._Disposed) {
            this._Disposed = true;

            this._FactoryChangeTokenRegistration?.Dispose();
            this._FactoryChangeTokenRegistration = default;

            this._FiltersChangeTokenRegistration?.Dispose();
            this._FiltersChangeTokenRegistration = default;
        }
    }

    private sealed class DisposingTracorScopedFilterFactory : ITracorScopedFilterFactory {
        private readonly ITracorScopedFilterFactory _TracorFactory;
        private readonly ServiceProvider _ServiceProvider;

        public DisposingTracorScopedFilterFactory(
            ITracorScopedFilterFactory tracorFactory,
            ServiceProvider serviceProvider) {
            this._TracorFactory = tracorFactory;
            this._ServiceProvider = serviceProvider;
        }

        public void Dispose() {
            this._ServiceProvider.Dispose();
        }

        public ITracorScopedFilter CreateTracorScopedFilter(string categoryName) {
            return this._TracorFactory.CreateTracorScopedFilter(categoryName);
        }

        public void AddTracorScopedFilterSourceRegister(ITracorScopedFilterSource register) {
            this._TracorFactory.AddTracorScopedFilterSourceRegister(register);
        }
    }

    private string DebuggerToString() {
        return $"Providers = {this._ListSourceName.Length}, {this._TracorScopedFilterOptions.DebuggerToString()}";
    }

    private sealed class TracorFactoryDebugView(TracorScopedFilterFactory tracorFactory) {
        public List<string> Registrations => new(tracorFactory._ListSourceName);
        public bool Disposed => tracorFactory._Disposed;
        public TracorScopedFilterOptionsBySourceName FilterOptions => tracorFactory._TracorScopedFilterOptions;
    }
}
