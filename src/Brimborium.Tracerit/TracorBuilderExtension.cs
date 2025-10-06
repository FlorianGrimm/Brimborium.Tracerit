namespace Brimborium.Tracerit;

public static class TracorBuilderExtension {

    /// <summary>
    /// Adds the Tracor logger provider to the logging builder, enabling integration between logging and tracing.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static ITracorBuilder AddTracorLogger(
        this ITracorBuilder tracorBuilder,
        Action<TracorLoggerOptions>? configure = default) {
        tracorBuilder.Services.AddSingleton<LoggerTracorDataPool>(LoggerTracorDataPool.Create);
        tracorBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TracorLoggerProvider>());

        var optionsBuilder = tracorBuilder.Services.AddOptions<TracorLoggerOptions>();
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return tracorBuilder;
    }

    /// <summary>
    /// Add ITracorActivityListener for runtime or testtime.
    /// </summary>
    /// <param name="tracorBuilder">The service collection to add services to.</param>
    /// <param name="addTestTimeServices">true - testtime; false - runtime</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static ITracorBuilder AddTracorActivityListener(
        this ITracorBuilder tracorBuilder,
        bool addTestTimeServices,
        IConfiguration? configuration = null,
        Action<TracorActivityListenerOptions>? configure = null
        ) {
        if (addTestTimeServices) {
            return tracorBuilder.AddTesttimeTracorActivityListener(configuration, configure);
        } else {
            return tracorBuilder.AddRuntimeTracorActivityListener(configuration, configure);
        }
    }

    /// <summary>
    /// Add ITracorActivityListener for runtime.
    /// </summary>
    /// <param name="tracorBuilder">The service collection to add services to.</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static ITracorBuilder AddRuntimeTracorActivityListener(
        this ITracorBuilder tracorBuilder,
        IConfiguration? configuration = null,
        Action<TracorActivityListenerOptions>? configure = null
        ) {
        // add runtime do nothing implementations
        tracorBuilder.Services.AddSingleton<ActivityTracorDataPool>(ActivityTracorDataPool.Create);

        tracorBuilder.Services.AddSingleton<RuntimeTracorActivityListener>();
        tracorBuilder.Services.AddSingleton<ITracorActivityListener>(
            (sp) => sp.GetRequiredService<RuntimeTracorActivityListener>());

        // options configure
        var optionsBuilder = tracorBuilder.Services.AddOptions<TracorActivityListenerOptions>();
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return tracorBuilder;
    }

    /// <summary>
    /// Add ITracorActivityListener for testtime.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static ITracorBuilder AddTesttimeTracorActivityListener(
        this ITracorBuilder tracorBuilder,
        IConfiguration? configuration = null,
        Action<TracorActivityListenerOptions>? configure = null
        ) {
        tracorBuilder.Services.AddSingleton<ActivityTracorDataPool>(ActivityTracorDataPool.Create);

        var configurationSection = configuration;
        tracorBuilder.Services.AddSingleton<TesttimeTracorActivityListener>((IServiceProvider serviceProvider) => TesttimeTracorActivityListener.Create(serviceProvider, configuration));
        tracorBuilder.Services.AddSingleton<ITracorActivityListener>(
            (sp) => sp.GetRequiredService<TesttimeTracorActivityListener>());

        // options configure
        var optionsBuilder = tracorBuilder.Services.AddOptions<TracorActivityListenerOptions>();
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return tracorBuilder;
    }

    /// <summary>
    /// Add singleton T
    /// </summary>
    /// <typeparam name="T">Type inherit <see cref="T:AddActivitySourceBase"/>.</typeparam>
    /// <param name="tracorBuilder">The service collection to add services to.</param>
    /// <returns>fluent this</returns>
    public static ITracorBuilder AddTracorInstrumentation<T>(
        this ITracorBuilder tracorBuilder
        )
        where T : InstrumentationBase {
        tracorBuilder.Services.AddTracorInstrumentation<T>();
        return tracorBuilder;
    }

    /// <summary>
    /// Add singleton T
    /// </summary>
    /// <typeparam name="T">Type inherit <see cref="T:AddActivitySourceBase"/>.</typeparam>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>fluent this</returns>
    public static IServiceCollection AddTracorInstrumentation<T>(
        this IServiceCollection servicebuilder
        )
        where T : InstrumentationBase {
        servicebuilder.AddSingleton<T>();
        servicebuilder.AddOptions<TracorActivityListenerOptions>()
            .Configure((options) => {
                options.ListActivitySourceResolver.Add(new InstrumentationBaseResolver<T>());
            });
        return servicebuilder;
    }

}
