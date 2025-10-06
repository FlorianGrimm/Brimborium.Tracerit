namespace Brimborium.Tracerit.Filter;

/// <summary>
/// The options for a TracorFilter.
/// </summary>
[DebuggerDisplay("{DebuggerToString(),nq}")]
public sealed class TracorScopedFilterOptions {
    /// <summary>
    /// Creates a new <see cref="TracorScopedFilterOptions"/> instance.
    /// </summary>
    public TracorScopedFilterOptions() { }

    /// <summary>
    /// Gets or sets the minimum level of log messages if none of the rules match.
    /// </summary>
    public LogLevel MinLevel { get; set; }

    /// <summary>
    /// Gets the collection of <see cref="TracorScopedFilterRule"/> used for filtering log messages.
    /// </summary>
    public IList<TracorScopedFilterRule> Rules => this.RulesInternal;

    // Concrete representation of the rule list
    internal List<TracorScopedFilterRule> RulesInternal { get; } = new List<TracorScopedFilterRule>();

    internal string DebuggerToString() {
        string debugText;
        if (this.MinLevel != LogLevel.None) {
            debugText = $"MinLevel = {this.MinLevel}";
        } else {
            // Display "Enabled = false". This makes it clear that the entire ITracor
            // is disabled and nothing is written.
            //
            // If "MinLevel = None" was displayed then someone could think that the
            // min level is disabled and everything is written.
            debugText = $"Enabled = false";
        }

        if (this.Rules.Count > 0) {
            debugText += $", Rules = {this.Rules.Count}";
        }

        return debugText;
    }
}
