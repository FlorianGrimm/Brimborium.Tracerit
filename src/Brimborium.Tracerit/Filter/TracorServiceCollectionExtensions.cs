namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up logging services in an <see cref="IServiceCollection" />.
/// </summary>
public static partial class TracorServiceCollectionExtensions {
    /// <summary>
    /// Adds logging services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddTracorScopedFilter(
        this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);
        
        var found = services.FirstOrDefault(
            sd => typeof(ITracorScopedFilterFactory) == sd.ServiceType
            ) is { };

        if (found) {
            // not again
        } else {
            services.AddOptions();

            services.TryAdd(ServiceDescriptor.Singleton<ITracorScopedFilterFactory, TracorScopedFilterFactory>());
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ITracorScopedFilter<>), typeof(TracorScopedFilter<>)));

#warning parameter LogLevel or add extension
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TracorScopedFilterOptions>>(
                new DefaultTracorScopedFilterOptions(LogLevel.Information)));
        }

        return services;
    }

    /// <summary>
    /// Adds logging services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configure">The <see cref="ITracorScopedFilterBuilder"/> configuration delegate.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddTracorScopedFilter(
        this IServiceCollection services,
        Action<ITracorScopedFilterBuilder> configure) {
        ArgumentNullException.ThrowIfNull(services);

        AddTracorScopedFilter(services);

        configure(new TracorScopedFilterBuilder(services));

        return services;
    }
}
