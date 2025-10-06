namespace Brimborium.Tracerit.Filter;

internal readonly struct TracorScopedFilterCategotryInformation {
    public TracorScopedFilterCategotryInformation(ITracorScopedFilterSource source, string category) {
        this.SourceName = source.GetSourceName();
        //this.Source = source;
        this.Category = category;
    }
    public TracorScopedFilterCategotryInformation(string sourceName, string category) {
        this.SourceName = sourceName;
        this.Category = category;
    }

    public string SourceName { get; }
    public string Category { get; }
    //public ITracorScopedFilterSource Source { get; }
}
