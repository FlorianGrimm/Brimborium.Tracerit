using Brimborium.Tracerit.TracorActivityListener;

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
    public static IServiceCollection AddTracor(
        this IServiceCollection servicebuilder,
        bool addTestTimeServices,
        Action<TracorValidatorOptions>? configure = default) {
        if (addTestTimeServices) {
            return servicebuilder.AddTesttimeTracor(configure);
        } else {
            return servicebuilder.AddRuntimeTracor(configure);
        }
    }

    /// <summary>
    /// Adds runtime Tracor services to the service collection. Runtime Tracor is optimized for production use with minimal overhead.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddRuntimeTracor(
        this IServiceCollection servicebuilder,
        Action<TracorValidatorOptions>? configure = default) {
        servicebuilder.AddSingleton<ITracor, RuntimeTracor>();
        servicebuilder.AddSingleton<RuntimeTracorValidator>();
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<RuntimeTracorValidator>());
        var optionsBuilder = servicebuilder.AddOptions<TracorValidatorOptions>();
        servicebuilder.AddSingleton(typeof(LazyCreatedLogger<>));

        if (configure is { }) { optionsBuilder.Configure(configure); }

        return servicebuilder;
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
        Action<TracorValidatorOptions>? configure = default) {
        servicebuilder.AddSingleton<ITracor, TesttimeTracor>();
        servicebuilder.AddSingleton<TesttimeTracorValidator>();
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<TesttimeTracorValidator>());
        var optionsBuilder = servicebuilder.AddOptions<TracorValidatorOptions>();
        servicebuilder.AddSingleton(typeof(LazyCreatedLogger<>));

        if (configure is { }) { optionsBuilder.Configure(configure); }

        return servicebuilder;
    }

    /// <summary>
    /// Adds the Tracor logger provider to the logging builder, enabling integration between logging and tracing.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static IServiceCollection AddTracorLogger(
        this IServiceCollection servicebuilder,
        Action<TracorLoggerOptions>? configure = default) {
        servicebuilder.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TracorLoggerProvider>());

        var optionsBuilder = servicebuilder.AddOptions<TracorLoggerOptions>();
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return servicebuilder;
    }

    /// <summary>
    /// Add ITracorActivityListener for runtime or testtime.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="addTestTimeServices">true - testtime; false - runtime</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static IServiceCollection AddTracorActivityListener(
        this IServiceCollection servicebuilder,
        bool addTestTimeServices,
        Action<TracorActivityListenerOptions>? configure = null
        ) {
        if (addTestTimeServices) {
            return servicebuilder.AddTesttimeTracorActivityListener(configure);
        } else {
            return servicebuilder.AddRuntimeTracorActivityListener(configure);
        }
    }

    /// <summary>
    /// Add ITracorActivityListener for runtime.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static IServiceCollection AddRuntimeTracorActivityListener(
        this IServiceCollection servicebuilder,
        Action<TracorActivityListenerOptions>? configure = null
        ) {
        // add runtime do nothing implementations
        servicebuilder.AddSingleton<RuntimeTracorActivityListener>();
        servicebuilder.AddSingleton<ITracorActivityListener>(
            (sp) => sp.GetRequiredService<RuntimeTracorActivityListener>());

        // options configure
        var optionsBuilder = servicebuilder.AddOptions<TracorActivityListenerOptions>();
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return servicebuilder;
    }

    /// <summary>
    /// Add ITracorActivityListener for testtime.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static IServiceCollection AddTesttimeTracorActivityListener(
        this IServiceCollection servicebuilder,
        Action<TracorActivityListenerOptions>? configure = null
        ) {
        servicebuilder.AddSingleton<TesttimeTracorActivityListener>();
        servicebuilder.AddSingleton<ITracorActivityListener>(
            (sp) => sp.GetRequiredService<TesttimeTracorActivityListener>());

        // options configure
        var optionsBuilder = servicebuilder.AddOptions<TracorActivityListenerOptions>();
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return servicebuilder;
    }

    /// <summary>
    /// Add singleton T
    /// </summary>
    /// <typeparam name="T">Type inherit <see cref="T:AddActivitySourceBase"/>.</typeparam>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>fluent this</returns>
    public static IServiceCollection AddActivitySourceBase<T>(
        this IServiceCollection servicebuilder
        )
        where T : ActivitySourceBase {
        servicebuilder.AddSingleton<T>();
        servicebuilder.AddOptions<TracorActivityListenerOptions>()
            .Configure((options) => {
                options.ListActivitySourceByType.Add(typeof(T));
            });
        return servicebuilder;
    }
}
