namespace Brimborium.Tracerit.Filter;

internal sealed class TracorScopedFilterSourceConfigurationFactory : ITracorScopedFilterConfigurationFactory {
    private readonly IEnumerable<TracorScopedFilterConfiguration> _Configurations;

    public TracorScopedFilterSourceConfigurationFactory(IEnumerable<TracorScopedFilterConfiguration> configurations) {
        this._Configurations = configurations;
    }

    public IConfiguration GetConfiguration(Type sourceType) {
        ArgumentNullException.ThrowIfNull(sourceType);
        var name = TracorScopedFilterSource.GetSourceNameFromType(sourceType);
        var configurationBuilder = new ConfigurationBuilder();
        foreach (TracorScopedFilterConfiguration configuration in this._Configurations) {
            var sectionFromName = configuration.Configuration.GetSection(name);
            configurationBuilder.AddConfiguration(sectionFromName);
        }
        return configurationBuilder.Build();
    }
}
