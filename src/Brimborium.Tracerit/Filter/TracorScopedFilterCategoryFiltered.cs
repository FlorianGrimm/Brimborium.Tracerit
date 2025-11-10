namespace Brimborium.Tracerit.Filter;

internal readonly struct TracorScopedFilterCategoryFiltered {
    public TracorScopedFilterCategoryFiltered(
        string sourceName,
        string category,
        LogLevel? minLevel,
        Func<string?, string?, LogLevel, bool>? filter) {
        this.Category = category;
        this.SourceName = sourceName;
        this.MinLevel = minLevel;
        this.Filter = filter;
    }

    public readonly string SourceName;

    public readonly string Category;
    
    public readonly LogLevel? MinLevel;

    public readonly Func<string?, string?, LogLevel, bool>? Filter;

    public bool IsEnabled(string sourceName, LogLevel level) {
        if (this.MinLevel != null && level < this.MinLevel) {
            return false;
        }

        //if (!string.Equals(sourceName, this.SourceName, StringComparison.OrdinalIgnoreCase)) {
        //    return false;
        //}

        if (this.Filter != null) {
            return this.Filter(this.SourceName, this.Category, level);
        }

        return true;
    }
}
