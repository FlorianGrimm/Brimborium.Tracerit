namespace Brimborium.Tracerit.Filter;

internal sealed class DefaultTracorScopedFilterOptions : ConfigureOptions<TracorScopedFilterOptions> {
    public DefaultTracorScopedFilterOptions(LogLevel level) : base(options => options.MinLevel = level) {
    }
}
