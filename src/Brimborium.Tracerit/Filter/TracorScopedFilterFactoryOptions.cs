#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace Brimborium.Tracerit.Filter;

public sealed class TracorScopedFilterFactoryOptions {
    public List<string> ListSourceName { get; }

    public TracorScopedFilterFactoryOptions() {
        this.ListSourceName = [];
    }
}
