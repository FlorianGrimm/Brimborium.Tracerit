namespace Brimborium.Tracerit;

/// <summary>
/// Provides extension methods for configuring Tracor services in the dependency injection container.
/// </summary>
public static class TracorServiceBuilderExtension {
    /// <summary>
    /// Adds runtime or testtime Tracor services to the service collection.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTracor(this IServiceCollection servicebuilder, bool addTestTimeServices) {
        if (addTestTimeServices) {
            return servicebuilder.AddTesttimeTracor();
        } else { 
            return servicebuilder.AddRuntimeTracor();
        }
    }

    /// <summary>
    /// Adds runtime Tracor services to the service collection. Runtime Tracor is optimized for production use with minimal overhead.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddRuntimeTracor(this IServiceCollection servicebuilder) {
        servicebuilder.AddSingleton<ITracor, RuntimeTracor>();
        servicebuilder.AddSingleton<RuntimeTracorValidator>();
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<RuntimeTracorValidator>());
        servicebuilder.AddSingleton(typeof(LazyCreatedLogger<>));
        return servicebuilder;
    }

    /// <summary>
    /// Adds test-time Tracor services to the service collection with default configuration.
    /// Test-time Tracor is designed for testing scenarios with full validation capabilities.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTesttimeTracor(this IServiceCollection servicebuilder) {
        return servicebuilder.AddTesttimeTracor(configure: (options) => { });
    }

    /// <summary>
    /// Adds test-time Tracor services to the service collection with custom configuration.
    /// Test-time Tracor is designed for testing scenarios with full validation capabilities.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="configure">An action to configure the Tracor validator options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTesttimeTracor(
        this IServiceCollection servicebuilder,
        Action<TracorValidatorOptions> configure) {
        servicebuilder.AddLogging();
        servicebuilder.AddSingleton<ITracor, TesttimeTracor>();
        servicebuilder.AddSingleton<TesttimeTracorValidator>();
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<TesttimeTracorValidator>());
        var optionsBuilder = servicebuilder.AddOptions<TracorValidatorOptions>();
        servicebuilder.AddSingleton(typeof(LazyCreatedLogger<>));

        if (configure is { }) { optionsBuilder.Configure(configure); }

        return servicebuilder;
    }
}
