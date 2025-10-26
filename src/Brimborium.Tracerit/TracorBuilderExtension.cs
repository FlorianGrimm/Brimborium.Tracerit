using Brimborium.Tracerit.FileSink;

namespace Brimborium.Tracerit;

public static class TracorBuilderExtension {

    internal static IConfiguration GetConfigurationTracorSection(IConfigurationRoot configuration) {
        return configuration.GetSection("Tracor");
    }
    internal static IConfiguration GetConfigurationTracorSinkFileSection(IConfigurationRoot configuration) {
        return configuration.GetSection("Tracor").GetSection("SinkFile");
    }


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
            return tracorBuilder.AddEnabledTracorActivityListener(configuration, configure);
        } else {
            return tracorBuilder.AddDisabledTracorActivityListener(configuration, configure);
        }
    }

    /// <summary>
    /// Add ITracorActivityListener for runtime.
    /// </summary>
    /// <param name="tracorBuilder">The service collection to add services to.</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static ITracorBuilder AddDisabledTracorActivityListener(
        this ITracorBuilder tracorBuilder,
        IConfiguration? configuration = null,
        Action<TracorActivityListenerOptions>? configure = null
        ) {
        // add runtime do nothing implementations
        tracorBuilder.Services.AddSingleton<ActivityTracorDataPool>(ActivityTracorDataPool.Create);

        tracorBuilder.Services.AddSingleton<DisabledTracorActivityListener>();
        tracorBuilder.Services.AddSingleton<ITracorActivityListener>(
            (sp) => sp.GetRequiredService<DisabledTracorActivityListener>());

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
    public static ITracorBuilder AddEnabledTracorActivityListener(
        this ITracorBuilder tracorBuilder,
        IConfiguration? configuration = null,
        Action<TracorActivityListenerOptions>? configure = null
        ) {
        tracorBuilder.Services.AddSingleton<ActivityTracorDataPool>(ActivityTracorDataPool.Create);

        var configurationSection = configuration;
        tracorBuilder.Services.AddSingleton<EnabledTracorActivityListener>(
            (IServiceProvider serviceProvider) => EnabledTracorActivityListener.Create(
                serviceProvider, configuration));
        tracorBuilder.Services.AddSingleton<ITracorActivityListener>(
            (sp) => sp.GetRequiredService<EnabledTracorActivityListener>());

        // options configure
        if (configuration is { }) {
            tracorBuilder.Services.AddOptions<TracorActivityListenerOptions>()
                .Bind(configuration);
        }
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

    public static ITracorBuilder AddTracorScopedFilter(
            this ITracorBuilder tracorBuilder,
            Action<ITracorScopedFilterBuilder>? configure) {
        if (configure is null) {
            tracorBuilder.Services.AddTracorScopedFilter();
        } else {
            tracorBuilder.Services.AddTracorScopedFilter(configure);
        }
        return tracorBuilder;
    }

    internal static ITracorBuilder AddFileTracorCollectiveSinkServices(
        this ITracorBuilder tracorBuilder) {
        //tracorBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ITracorCollectiveSink, FileTracorCollectiveSink>());
        tracorBuilder.Services.AddSingleton<FileTracorCollectiveSink>();
        tracorBuilder.Services.Add(
            ServiceDescriptor.Singleton<ITracorCollectiveSink>(
                static (IServiceProvider sp) => sp.GetRequiredService<FileTracorCollectiveSink>()));
        tracorBuilder.Services.Add(ServiceDescriptor.Transient(
            typeof(ITracorSink<>), typeof(TracorSink<>)));
        return tracorBuilder;
    }

    /// <summary>
    /// Add a file persitence for tracor
    /// </summary>
    /// <param name="tracorBuilder">that</param>
    /// <param name="configuration">optional: the configurationroot:Tracor:SinkFile will be used</param>
    /// <param name="configure">optional: additional configuration - allows to set GetApplicationStopping</param>
    /// <returns></returns>
    /// <example>
    /// .AddFileTracorCollectiveSinkDefault(
    ///    configuration: builder.Configuration,
    ///               configure: (fileTracorOptions) => {
    ///                   fileTracorOptions.GetApplicationStopping = static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping
    ///})
    /// </example>
    public static ITracorBuilder AddFileTracorCollectiveSinkDefault(
        this ITracorBuilder tracorBuilder,
        IConfigurationRoot? configuration = default,
        Action<FileTracorOptions>? configure = default) {
        var optionsBuilder = tracorBuilder.Services.AddOptions<FileTracorOptions>();
        if (configuration is { }) {
            optionsBuilder.Bind(GetConfigurationTracorSinkFileSection(configuration));
        }
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return tracorBuilder.AddFileTracorCollectiveSinkServices();
    }


    public static ITracorBuilder AddFileTracorCollectiveSinkCustom(
        this ITracorBuilder tracorBuilder,
        IConfiguration? configuration = default,
        Action<FileTracorOptions>? configure = default) {
        var optionsBuilder = tracorBuilder.Services.AddOptions<FileTracorOptions>();
        if (configuration is { }) {
            optionsBuilder.Bind(configuration);
        }
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return tracorBuilder.AddFileTracorCollectiveSinkServices();
    }


    public static ITracorBuilder AddTracorValidatorService(
        this ITracorBuilder tracorBuilder,
        Action<TracorValidatorServiceOptions>? configure = default) {
        tracorBuilder.Services.AddSingleton<TracorValidatorService>();

        var optionsBuilder = tracorBuilder.Services.AddOptions<TracorValidatorServiceOptions>();
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }

        return tracorBuilder;
    }

}
