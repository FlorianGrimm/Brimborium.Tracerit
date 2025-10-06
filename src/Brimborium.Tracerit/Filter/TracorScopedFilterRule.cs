namespace Brimborium.Tracerit.Filter;

/// <summary>
/// Defines a rule used to filter log messages
/// </summary>
public sealed class TracorScopedFilterRule {
    /// <summary>
    /// Creates a new <see cref="TracorScopedFilterRule"/> instance.
    /// </summary>
    /// <param name="providerName">The provider name to use in this filter rule.</param>
    /// <param name="categoryName">The category name to use in this filter rule.</param>
    /// <param name="logLevel">The <see cref="LogLevel"/> to use in this filter rule.</param>
    /// <param name="filter">The filter to apply.</param>
    public TracorScopedFilterRule(string? providerName, string? categoryName, LogLevel? logLevel, Func<string?, string?, LogLevel, bool>? filter) {
        this.SourceName = providerName;
        this.CategoryName = categoryName;
        this.LogLevel = logLevel;
        this.Filter = filter;
    }

    /// <summary>
    /// Gets the tracor provider type or alias this rule applies to.
    /// </summary>
    public string? SourceName { get; }

    /// <summary>
    /// Gets the tracor category this rule applies to.
    /// </summary>
    public string? CategoryName { get; }

    /// <summary>
    /// Gets the minimum <see cref="LogLevel"/> of messages.
    /// </summary>
    public LogLevel? LogLevel { get; }

    /// <summary>
    /// Gets the filter delegate that would be applied to messages that passed the <see cref="LogLevel"/>.
    /// </summary>
    public Func<string?, string?, LogLevel, bool>? Filter { get; }

    /// <inheritdoc/>
    public override string ToString() {
        return $"{nameof(this.SourceName)}: '{this.SourceName}', {nameof(this.CategoryName)}: '{this.CategoryName}', {nameof(this.LogLevel)}: '{this.LogLevel}', {nameof(this.Filter)}: '{this.Filter}'";
    }
}
