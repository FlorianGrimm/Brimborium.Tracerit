namespace Brimborium.Tracerit.Filter;

internal readonly struct TracorScopedFilterCategoryInformation {
    public TracorScopedFilterCategoryInformation(ITracorScopedFilterSource source, string category) {
        this.SourceName = source.GetSourceName();
        this.Category = category;
    }
    public TracorScopedFilterCategoryInformation(string sourceName, string category) {
        this.SourceName = sourceName;
        this.Category = category;
    }

    public string SourceName { get; }
    public string Category { get; }
}
