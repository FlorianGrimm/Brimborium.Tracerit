#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace Brimborium.Tracerit.Filter;

/// <summary>
/// Produces instances of <see cref="ITracorScopedFilter"/> classes based on the given providers.
/// </summary>
[DebuggerDisplay("{DebuggerToString(),nq}")]
[DebuggerTypeProxy(typeof(TracorFactoryDebugView))]
public class TracorScopedFilterFactory : ITracorScopedFilterFactory {
    private readonly ConcurrentDictionary<string, TracorScopedFilter> _DictFilterByName = new(StringComparer.Ordinal);
    private readonly List<string> _Registrations = [];
    private string[] _ListSourceName = [];
    private readonly Lock _Lock = new();
    private bool _Disposed;
    private List<string> _OptionsListSourceName;
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
        this._OptionsListSourceName = [];

        foreach (var scopedTracorSource in listScopedTracorSource.OrderBy(i => i.GetSourceName())) {
            this.AddTracorScopedFilterSource(scopedTracorSource, dispose: false);
        }

        this._FactoryChangeTokenRegistration = factoryOptions.OnChange(this.RefreshFactoryOption);
        this.RefreshFactoryOption(factoryOptions.CurrentValue);

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
        this._OptionsListSourceName.Clear();
        this._OptionsListSourceName.AddRange(options.ListSourceName);
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
                        if (this._DictFilterByName.ContainsKey(categoryName)) {
                            //
                        } else {
                            var filter = new TracorScopedFilter(categoryName, []);
                            this._DictFilterByName[categoryName] = filter;
                            filter.ListCategotryInformation = this.CreateTracors(categoryName);
                            filter.ListFilteredTracors = this.ApplyFiltersWithinLock(filter.ListCategotryInformation, tracorScopedFilterOptions);
                        }
                    }
                }
            }
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
                foreach (KeyValuePair<string, TracorScopedFilter> kvFilter in this._DictFilterByName) {
                    TracorScopedFilter tracor = kvFilter.Value;
                    tracor.ListFilteredTracors = this.ApplyFiltersWithinLock(tracor.ListCategotryInformation, tracorScopedFilterOptions);
                }
            }
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

        if (!this._DictFilterByName.TryGetValue(categoryName, out TracorScopedFilter? tracor)) {
            using (this._Lock.EnterScope()) {
                if (!this._DictFilterByName.TryGetValue(categoryName, out tracor)) {
                    tracor = new TracorScopedFilter(categoryName, []);
                    this._DictFilterByName[categoryName] = tracor;
                    var listTracorInformation = this.CreateTracors(categoryName);
                    tracor.ListCategotryInformation = listTracorInformation;
                    tracor.ListFilteredTracors = this.ApplyFiltersWithinLock(
                        listTracorInformation, this._TracorScopedFilterOptions);
                }
            }
        }

        return tracor;
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

            foreach (KeyValuePair<string, TracorScopedFilter> existingTracor in this._DictFilterByName) {
                TracorScopedFilter tracor = existingTracor.Value;
                TracorScopedFilterCategotryInformation[] tracorInformation = tracor.ListCategotryInformation;
                var sourceName = tsfSourceName.GetSourceName();

                int length = tracorInformation.Length;
                int newTracorIndex;
                var item = new TracorScopedFilterCategotryInformation(
                        sourceName: sourceName,
                        category: existingTracor.Key);
                if (length == 0) {
                    newTracorIndex = length;

                    Array.Resize(ref tracorInformation, length + 1);
                    tracorInformation[newTracorIndex] = item;
                } else {
                    newTracorIndex = length;
                    for (int idx = 0; idx < length; idx++) {
                        var cmp = StringComparer.OrdinalIgnoreCase.Compare(
                            tracorInformation[idx].SourceName,
                            sourceName
                            );
                        if (cmp > 0) {
                            newTracorIndex = idx;
                            break;
                        }
                    }
                    Array.Resize(ref tracorInformation, length + 1);
                    for (int idx = length; idx > newTracorIndex; idx--) {
                        tracorInformation[idx] = tracorInformation[idx - 1];
                    }
                    tracorInformation[newTracorIndex] = item;
                }

                tracor.ListCategotryInformation = tracorInformation;
                tracor.ListFilteredTracors = this.ApplyFiltersWithinLock(tracorInformation, tracorScopedFilterOptions);
            }
        }
    }

    private void AddTracorScopedFilterSource(ITracorScopedFilterSource tsfSourceName, bool dispose) {
        var sourceName = tsfSourceName.GetSourceName();

        var pos = this._Registrations.BinarySearch(sourceName);
        if (0 <= pos) {
            this._Registrations.Insert(pos, sourceName);
        } else {
            this._Registrations.Insert(~pos, sourceName);
        }

        this._ListSourceName = this.CreateListSourceName();
    }

    private string[] CreateListSourceName() {
        int count = this._Registrations.Count + this._OptionsListSourceName.Count;
        HashSet<string> result = new(count, StringComparer.OrdinalIgnoreCase);

        foreach (var name in this._Registrations) { result.Add(name); }
        foreach (var name in this._OptionsListSourceName) { result.Add(name); }

        return result
            .OrderBy(a => a, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private TracorScopedFilterCategotryInformation[] CreateTracors(string categoryName) {
        int count = this._Registrations.Count;
        var tracors = new List<TracorScopedFilterCategotryInformation>(count);
        for (int idx = 0; idx < count; idx++) {
            var tracorInformation = new TracorScopedFilterCategotryInformation(
                this._Registrations[idx],
                categoryName);
            tracors.Add(tracorInformation);

            // We do not need to check for NullTracor<T>.Instance as no provider would reasonably return it (the <T> handling is at
            // outer tracors level, not inner level tracors in Tracor/TracorProvider).
            //if (tracorInformation.Source != Brimborium.Tracerit.Filter.Internal.NullTracorScopedFilterProvider.Instance) {
            //    tracors.Add(tracorInformation);
            //}
        }
        return tracors.ToArray();
    }

    private TracorScopedFilterCategoryFiltered[] ApplyFiltersWithinLock(
        TracorScopedFilterCategotryInformation[] tracors,
        TracorScopedFilterOptionsBySourceName tracorScopedFilterOptions) {
        var tracorsLength = tracors.Length;
        List<TracorScopedFilterCategoryFiltered> resultTracors = new(tracorsLength);

        for (int idxTracor = 0; idxTracor < tracorsLength; idxTracor++) {
            ref TracorScopedFilterCategotryInformation tracorInformation = ref tracors[idxTracor];
            TracorScopedFilterRuleSelector.Select(
                tracorScopedFilterOptions,
                tracorInformation.SourceName,
                tracorInformation.Category,
                out LogLevel? minLevel,
                out Func<string?, string?, LogLevel, bool>? filter);

            if (minLevel is not null and > LogLevel.Critical) {
                continue;
            }

            resultTracors.Add(
                new TracorScopedFilterCategoryFiltered(
                    tracorInformation.SourceName,
                    tracorInformation.Category,
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
        return $"Providers = {this._Registrations.Count}, {this._TracorScopedFilterOptions.DebuggerToString()}";
    }

    private sealed class TracorFactoryDebugView(TracorScopedFilterFactory tracorFactory) {
        public List<string> Registrations => new(tracorFactory._ListSourceName);
        public bool Disposed => tracorFactory._Disposed;
        public TracorScopedFilterOptionsBySourceName FilterOptions => tracorFactory._TracorScopedFilterOptions;
    }
}
