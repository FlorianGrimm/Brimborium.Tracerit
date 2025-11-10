namespace Brimborium.Tracerit.Filter;

internal sealed class TracorScopedFilterSourceConfiguration<T> : ITracorScopedFilterSourceConfiguration<T> {
    public TracorScopedFilterSourceConfiguration(ITracorScopedFilterConfigurationFactory providerConfigurationFactory) {
        this.Configuration = providerConfigurationFactory.GetConfiguration(typeof(T));
    }

    public IConfiguration Configuration { get; }
}
