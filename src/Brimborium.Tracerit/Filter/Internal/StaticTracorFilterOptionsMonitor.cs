namespace Brimborium.Tracerit.Filter.Internal;

internal sealed class StaticTracorScopedFilterOptionsMonitor
    : IOptionsMonitor<TracorScopedFilterOptions> {
    public StaticTracorScopedFilterOptionsMonitor(TracorScopedFilterOptions currentValue) {
        this.CurrentValue = currentValue ?? throw new ArgumentNullException(nameof(currentValue));
    }

    public IDisposable? OnChange(Action<TracorScopedFilterOptions, string> listener) => null;

    public TracorScopedFilterOptions Get(string? name) => this.CurrentValue;

    public TracorScopedFilterOptions CurrentValue { get; }
}
