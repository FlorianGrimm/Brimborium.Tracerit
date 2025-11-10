namespace Brimborium.Tracerit.Filter;

internal sealed class TracorScopedFilter : ITracorScopedFilter {
    private readonly string _CategoryName;

    public TracorScopedFilter(string categoryName, TracorScopedFilterCategoryInformation[] listCategoryInformation) {
        this._CategoryName = categoryName;
        this.ListCategoryInformation = listCategoryInformation;
    }

    public TracorScopedFilterCategoryInformation[] ListCategoryInformation;
    public TracorScopedFilterCategoryFiltered[]? ListFilteredTracors;


    public bool IsEnabled(string sourceName, LogLevel logLevel) {
        TracorScopedFilterCategoryFiltered[]? listFilteredTracors = this.ListFilteredTracors;
        if (listFilteredTracors == null) {
            return false;
        }

        for (int indexFilteredTracors = 0; indexFilteredTracors < listFilteredTracors.Length; indexFilteredTracors++) {
            ref readonly TracorScopedFilterCategoryFiltered tracorInfo = ref listFilteredTracors[indexFilteredTracors];

            var cmp = StringComparer.OrdinalIgnoreCase.Compare(tracorInfo.SourceName, sourceName);
            if (cmp < 0) {
                continue;
            } else if (cmp > 0) {
                break;
            } else {
                return tracorInfo.IsEnabled(sourceName, logLevel);
            }
        }
        return false;
    }

    private bool _IncludingAllSubScope = false;

    public bool IncludingAllSubScope() => this._IncludingAllSubScope;

    public void SetIncludingAllSubScope(bool value) {
        this._IncludingAllSubScope = value;
    }
}

public class TracorScopedFilter<T> : ITracorScopedFilter<T> {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly ITracorScopedFilter _TracorScopedFilter;

    /// <summary>
    /// Creates a new <see cref="TracorScopedFilter{T}"/>.
    /// </summary>
    /// <param name="factory">The factory.</param>
    public TracorScopedFilter(ITracorScopedFilterFactory factory) {
        ArgumentNullException.ThrowIfNull(factory);

        this._TracorScopedFilter = factory.CreateTracorScopedFilter(GetCategoryName());
    }

    /// <inheritdoc />
    bool ITracorScopedFilter.IsEnabled(string sourceName, LogLevel logLevel) {
        return this._TracorScopedFilter.IsEnabled(sourceName, logLevel);
    }

    /// <inheritdoc />
    public bool IncludingAllSubScope() 
        => this._TracorScopedFilter.IncludingAllSubScope();

    private static string GetCategoryName() => TypeNameHelper.GetTypeDisplayName(typeof(T), includeGenericParameters: false, nestedTypeDelimiter: '.');
}
