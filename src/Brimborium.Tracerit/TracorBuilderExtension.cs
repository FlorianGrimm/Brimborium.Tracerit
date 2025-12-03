namespace Brimborium.Tracerit;

public static class TracorBuilderExtension {

    internal static IConfiguration GetConfigurationTracorSection(IConfiguration configuration) {
        return configuration.GetSection("Tracor");
    }
    internal static IConfiguration GetConfigurationTracorFileSinkSection(IConfiguration configuration) {
        return configuration.GetSection("Tracor").GetSection("FileSink");
    }
    internal static IConfiguration GetConfigurationTracorHttpSinkSection(IConfiguration configuration) {
        return configuration.GetSection("Tracor").GetSection("HttpSink");
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
        tracorBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TracorLoggerProvider>());

        var optionsBuilder = tracorBuilder.Services.AddOptions<TracorLoggerOptions>();
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return tracorBuilder;
    }

    /// <summary>
    /// Add ITracorActivityListener for disabled/runtime or enabled/test-time.
    /// </summary>
    /// <param name="tracorBuilder">The service collection to add services to.</param>
    /// <param name="enabled">true - enabled/test-time; false - disabled/runtime</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static ITracorBuilder AddTracorActivityListener(
        this ITracorBuilder tracorBuilder,
        bool enabled,
        IConfiguration? configuration,
        Action<TracorActivityListenerOptions>? configure
        ) {
        if (enabled) {
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
        IConfiguration? configuration,
        Action<TracorActivityListenerOptions>? configure
        ) {
        // add runtime do nothing implementations
        tracorBuilder.Services.AddSingleton<TracorDataRecordPool>();
        tracorBuilder.Services.AddSingleton<DisabledTracorActivityListener>();
        tracorBuilder.Services.AddSingleton<ITracorActivityListener>(
            (sp) => sp.GetRequiredService<DisabledTracorActivityListener>());

        // options configure
        var optionsBuilder = tracorBuilder.Services.AddOptions<TracorActivityListenerOptions>();
        if (configuration is { }) {
            optionsBuilder.Bind(configuration);
        }
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }

        return tracorBuilder;
    }

    /// <summary>
    /// Add ITracorActivityListener for disabled/test-time.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="configure">configure options</param>
    /// <returns>fluent this</returns>
    public static ITracorBuilder AddEnabledTracorActivityListener(
        this ITracorBuilder tracorBuilder,
        IConfiguration? configuration,
        Action<TracorActivityListenerOptions>? configure
        ) {
        tracorBuilder.Services.AddSingleton<TracorDataRecordPool>();

        var configurationSection = configuration;
        tracorBuilder.Services.AddSingleton<EnabledTracorActivityListener>();
        tracorBuilder.Services.AddSingleton<ITracorActivityListener>((sp) => sp.GetRequiredService<EnabledTracorActivityListener>());

        // options configure
        var optionsBuilder = tracorBuilder.Services.AddOptions<TracorActivityListenerOptions>();
        if (configuration is { }) {
            optionsBuilder.Bind(configuration);
        }
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
    /// TODO
    /// </summary>
    /// <param name="tracorBuilder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
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

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="tracorBuilder"></param>
    /// <returns></returns>
    internal static ITracorBuilder AddFileTracorCollectiveSinkServices(
        this ITracorBuilder tracorBuilder) {
        foreach (var serviceDescriptor in tracorBuilder.Services) {
            if (typeof(TracorCollectiveFileSink).Equals(serviceDescriptor.ServiceType)) {
                return tracorBuilder;
            }
        }
        //tracorBuilder.Services.Add(ServiceDescriptor.Singleton<ITracorCollectiveSink, TracorCollectiveFileSink>());
        tracorBuilder.Services.AddSingleton<TracorCollectiveFileSink>();
        tracorBuilder.Services.AddSingleton<ITracorCollectiveSink>(
            (serviceProvider) => serviceProvider.GetRequiredService<TracorCollectiveFileSink>());
        return tracorBuilder;
    }

    /// <summary>
    /// Add a file persistence for tracor
    /// </summary>
    /// <param name="tracorBuilder">that</param>
    /// <param name="configuration">optional: the configuration root:Tracor:FileSink will be used</param>
    /// <param name="configure">optional: additional configuration - allows to set GetApplicationStopping</param>
    /// <returns></returns>
    /// <example>
    /// .AddFileTracorCollectiveSinkDefault(
    ///    configuration: builder.Configuration,
    ///               configure: (fileTracorOptions) => {
    ///                   fileTracorOptions.SetOnGetApplicationStopping(static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
    /// })
    /// </example>
    public static ITracorBuilder AddFileTracorCollectiveSinkDefault(
        this ITracorBuilder tracorBuilder,
        IConfigurationRoot? configuration,
        Action<TracorFileSinkOptions>? configure) {
        var optionsBuilder = tracorBuilder.Services.AddOptions<TracorFileSinkOptions>();
        if (configuration is { }) {
            optionsBuilder.Bind(GetConfigurationTracorFileSinkSection(configuration));
        }
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return tracorBuilder.AddFileTracorCollectiveSinkServices();
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="tracorBuilder"></param>
    /// <param name="configuration"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static ITracorBuilder AddFileTracorCollectiveSinkCustom(
        this ITracorBuilder tracorBuilder,
        IConfiguration? configuration = default,
        Action<TracorFileSinkOptions>? configure = default) {
        var optionsBuilder = tracorBuilder.Services.AddOptions<TracorFileSinkOptions>();
        if (configuration is { }) {
            optionsBuilder.Bind(configuration);
        }
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return tracorBuilder.AddFileTracorCollectiveSinkServices();
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="tracorBuilder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
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

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="tracorBuilder"></param>
    /// <param name="configuration"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static ITracorBuilder AddTracorCollectiveHttpSink(
        this ITracorBuilder tracorBuilder,
        IConfiguration? configuration,
        Action<TracorHttpSinkOptions>? configure) {
        tracorBuilder.AddTracorCollectiveHttpSinkServices();
        if (configuration is { } || configure is { }) {
            var optionBuilder = tracorBuilder.Services.AddOptions<TracorHttpSinkOptions>();
            if (configuration is { }) {
                //optionBuilder.BindConfiguration("Tracor:HttpSink");
                optionBuilder.Bind(GetConfigurationTracorHttpSinkSection(configuration));
            }
            if (configure is { }) {
                optionBuilder.Configure(configure);
            }
        }
        return tracorBuilder;
    }

    internal static ITracorBuilder AddTracorCollectiveHttpSinkServices(
        this ITracorBuilder tracorBuilder) {
        foreach (var serviceDescriptor in tracorBuilder.Services) {
            if (typeof(TracorCollectiveHttpSink).Equals(serviceDescriptor.ServiceType)) {
                return tracorBuilder;
            }
        }

        tracorBuilder.Services.AddSingleton<TracorCollectiveHttpSink>();
        tracorBuilder.Services.AddSingleton<ITracorCollectiveSink>(
            (serviceProvider) => serviceProvider.GetRequiredService<TracorCollectiveHttpSink>());

        return tracorBuilder;
    }
}
