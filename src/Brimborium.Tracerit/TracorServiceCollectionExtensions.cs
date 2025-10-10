namespace Microsoft.Extensions.DependencyInjection;

public static partial class TracorServiceCollectionExtensions {
    internal const string RequiresDynamicCodeMessage = "Binding TOptions to configuration values may require generating dynamic code at runtime.";
    internal const string TrimmingRequiresUnreferencedCodeMessage = "TOptions's dependent types may have their members trimmed. Ensure all required members are preserved.";

    /// <summary>
    /// Adds disabled(e.g runtime) or enabled (e.g. test-time) Tracor services to the service collection.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    ///    builder.Services.AddTracor(
    ///        addEnabledServices: tracorEnabled,
    ///            configureTracor: (tracorOptions) => {
    ///                tracorOptions.ApplicationName = "customized";
    ///            })
    ///            .AddFileTracorCollectiveSinkDefault(
    ///               configuration: builder.Configuration,
    ///               configure: (fileTracorOptions) => {
    ///                   fileTracorOptions.GetApplicationStopping = static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping
    ///               })
    ///            .AddTracorActivityListener(tracorEnabled)
    ///            .AddTracorInstrumentation<SampleInstrumentation>()
    ///            .AddTracorLogger()
    ///            ;
    /// </example>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ITracorBuilder AddTracor(
            this IServiceCollection servicebuilder,
            bool addEnabledServices,
            Action<TracorOptions>? configureTracor = default,
            Action<TracorDataConvertOptions>? configureConvert = default,
            string tracorScopedFilterSection = "") {
        if (addEnabledServices) {
            return servicebuilder.AddEnabledTracor(
                configureTracor,
                configureConvert,
                tracorScopedFilterSection);
        } else {
            return servicebuilder.AddDisabledTracor(
                configureTracor,
                configureConvert,
                tracorScopedFilterSection);
        }
    }

    /// <summary>
    /// Adds a disabled Tracor services to the service collection. 
    /// Disabled Tracor dows nothing - exists only so that the DI dont fail.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ITracorBuilder AddDisabledTracor(
        this IServiceCollection servicebuilder,
        Action<TracorOptions>? configureTracor = default,
        Action<TracorDataConvertOptions>? configureConvert = default,
        string tracorScopedFilterSection = "") {
        servicebuilder.AddSingleton<ITracorCollectivePublisher, TracorCollectivePublisher>();
        servicebuilder.AddSingleton<TracorDataRecordPool>(TracorDataRecordPool.Create);
        servicebuilder.AddSingleton<ITracorServiceSink, DisabledTracorServiceSink>();
        servicebuilder.AddSingleton<ITracorDataConvertService, TracorDataConvertService>();
        servicebuilder.AddSingleton<DisabledTracorValidator>();
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<DisabledTracorValidator>());
        servicebuilder.AddSingleton(typeof(LazyCreatedLogger<>));

        servicebuilder.AddTracorScopedFilter((builder) => {
            builder.AddTracorScopedFilterConfiguration(tracorScopedFilterSection);
        });

        var optionsBuilder = servicebuilder.AddOptions<TracorDataConvertOptions>();
        if (configureConvert is { }) { optionsBuilder.Configure(configureConvert); }

        return new TracorBuilder(servicebuilder);
    }

    /// <summary>
    /// Adds the enabled Tracor services to the service collection with custom configuration.
    /// Enabled Tracor is designed for testing scenarios with full validation capabilities.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="configureConvert">An action to configure the Tracor validator options.</param>
    /// <returns>The service collection for method chaining.</returns>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ITracorBuilder AddEnabledTracor(
        this IServiceCollection servicebuilder,
        Action<TracorOptions>? configureTracor = default,
        Action<TracorDataConvertOptions>? configureConvert = default,
        string tracorScopedFilterSection = "") {
        servicebuilder.AddSingleton<ITracorCollectivePublisher, TracorCollectivePublisher>();
        servicebuilder.AddSingleton<ActivityTracorDataPool>(ActivityTracorDataPool.Create);
        servicebuilder.AddSingleton<TracorDataRecordPool>(TracorDataRecordPool.Create);
        servicebuilder.AddSingleton<ITracorServiceSink, TracorServiceSink>();
        servicebuilder.AddTransient<ITracorCollectiveSink>(
            static (sp) => sp.GetRequiredService<TracorValidator>());
        servicebuilder.AddSingleton<ITracorDataConvertService, TracorDataConvertService>();
        servicebuilder.AddSingleton<TracorValidator>(TracorValidator.Create);
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<TracorValidator>());
        servicebuilder.AddSingleton(typeof(LazyCreatedLogger<>));

        servicebuilder.AddTracorScopedFilter((builder) => {
            builder.AddTracorScopedFilterConfiguration(tracorScopedFilterSection);
        });

        var optionsBuilder = servicebuilder.AddOptions<TracorDataConvertOptions>();
        if (configureConvert is { }) { optionsBuilder.Configure(configureConvert); }

        return new TracorBuilder(servicebuilder);
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