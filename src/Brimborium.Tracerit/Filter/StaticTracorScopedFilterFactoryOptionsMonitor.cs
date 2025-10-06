#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace Brimborium.Tracerit.Filter;

internal sealed class StaticTracorScopedFilterFactoryOptionsMonitor
    : IOptionsMonitor<TracorScopedFilterFactoryOptions> {
    public StaticTracorScopedFilterFactoryOptionsMonitor(TracorScopedFilterFactoryOptions currentValue) {
        this.CurrentValue = currentValue ?? throw new ArgumentNullException(nameof(currentValue));
    }

    public IDisposable? OnChange(Action<TracorScopedFilterFactoryOptions, string> listener) => null;

    public TracorScopedFilterFactoryOptions Get(string? name) => this.CurrentValue;

    public TracorScopedFilterFactoryOptions CurrentValue { get; }
}
