namespace Brimborium.Tracerit.Filter;

internal sealed class TracorScopedFilterConfiguration {
    public IConfiguration Configuration { get; }

    public TracorScopedFilterConfiguration(IConfiguration configuration) {
        this.Configuration = configuration;
    }
}
