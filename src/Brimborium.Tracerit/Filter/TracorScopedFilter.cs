namespace Brimborium.Tracerit.Filter;

internal sealed class TracorScopedFilter : ITracorScopedFilter {
    private readonly string _CategoryName;

    public TracorScopedFilter(string categoryName, TracorScopedFilterCategotryInformation[] tracors) {
        this._CategoryName = categoryName;
        this.Tracors = tracors;
    }

    public TracorScopedFilterCategotryInformation[] Tracors;
    public TracorScopedFilterCategoryFiltered[]? FilteredTracors;

    public bool IsEnabled(string sourceName, LogLevel logLevel) {
        TracorScopedFilterCategoryFiltered[]? tracors = this.FilteredTracors;
        if (tracors == null) {
            return false;
        }

        for (int idxTracors = 0; idxTracors < tracors.Length; idxTracors++) {
            ref readonly TracorScopedFilterCategoryFiltered tracorInfo = ref tracors[idxTracors];

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

        this._tracor = factory.CreateTracor(GetCategoryName());
    }

    /// <inheritdoc />
    bool ITracorScopedFilter.IsEnabled(string sourceName, LogLevel logLevel) {
        return this._tracor.IsEnabled(sourceName, logLevel);
    }

    private static string GetCategoryName() => TypeNameHelper.GetTypeDisplayName(typeof(T), includeGenericParameters: false, nestedTypeDelimiter: '.');
}
