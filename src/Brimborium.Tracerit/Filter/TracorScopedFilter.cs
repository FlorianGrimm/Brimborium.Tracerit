namespace Brimborium.Tracerit.Filter;

internal sealed class TracorScopedFilter : ITracorScopedFilter {
    private readonly string _CategoryName;

    public TracorScopedFilter(string categoryName, TracorScopedFilterCategotryInformation[] listCategotryInformation) {
        this._CategoryName = categoryName;
        this.ListCategotryInformation = listCategotryInformation;
    }

    public TracorScopedFilterCategotryInformation[] ListCategotryInformation;
    public TracorScopedFilterCategoryFiltered[]? ListFilteredTracors;

    public bool IsEnabled(string sourceName, LogLevel logLevel) {
        TracorScopedFilterCategoryFiltered[]? listFilteredTracors = this.ListFilteredTracors;
        if (listFilteredTracors == null) {
            return false;
        }

        for (int idxFilteredTracors = 0; idxFilteredTracors < listFilteredTracors.Length; idxFilteredTracors++) {
            ref readonly TracorScopedFilterCategoryFiltered tracorInfo = ref listFilteredTracors[idxFilteredTracors];

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
}

public class TracorScopedFilter<T> : ITracorScopedFilter<T> {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly ITracorScopedFilter _tracor;

    /// <summary>
    /// Creates a new <see cref="TracorScopedFilter{T}"/>.
    /// </summary>
    /// <param name="factory">The factory.</param>
    public TracorScopedFilter(ITracorScopedFilterFactory factory) {
        ArgumentNullException.ThrowIfNull(factory);

        this._tracor = factory.CreateTracorScopedFilter(GetCategoryName());
    }

    /// <inheritdoc />
    bool ITracorScopedFilter.IsEnabled(string sourceName, LogLevel logLevel) {
        return this._tracor.IsEnabled(sourceName, logLevel);
    }

    private static string GetCategoryName() => TypeNameHelper.GetTypeDisplayName(typeof(T), includeGenericParameters: false, nestedTypeDelimiter: '.');
}
