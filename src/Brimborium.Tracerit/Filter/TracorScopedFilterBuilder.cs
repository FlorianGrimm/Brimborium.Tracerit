namespace Brimborium.Tracerit.Filter;

public sealed class TracorScopedFilterBuilder(IServiceCollection services) : ITracorScopedFilterBuilder {
    public IServiceCollection Services { get; } = services;
}
