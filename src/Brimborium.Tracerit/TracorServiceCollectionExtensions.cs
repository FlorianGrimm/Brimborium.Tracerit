#pragma warning disable IDE1006 // Naming Styles

namespace Microsoft.Extensions.DependencyInjection;

public static partial class TracorServiceCollectionExtensions {
    internal const string RequiresDynamicCodeMessage = "Binding TOptions to configuration values may require generating dynamic code at runtime.";
    internal const string TrimmingRequiresUnreferencedCodeMessage = "TOptions's dependent types may have their members trimmed. Ensure all required members are preserved.";

    /// <summary>
    /// Adds disabled(e.g runtime) or enabled (e.g. test-time) Tracor services to the service collection.
    /// </summary>
    /// <param name="serviceBuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    ///    builder.Services
    ///        .AddTracor(
    ///            addEnabledServices: tracorEnabled,
    ///            configureTracor: (tracorOptions) => {
    ///                tracorOptions.ApplicationName = "customized";
    ///                builder.Configuration.BindTracorOptionsDefault(tracorOptions);
    ///                tracorOptions.SetOnGetApplicationStopping(
    ///                    static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
    ///            },
    ///            configureConvert: default)
    ///        .AddFileTracorCollectiveSinkDefault(
    ///           configuration: builder.Configuration,
    ///           configure: default})
    ///        .AddTracorActivityListener(tracorEnabled)
    ///        .AddTracorInstrumentation<SampleInstrumentation>()
    ///        .AddTracorLogger()
    ///        ;
    /// </example>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ITracorBuilder AddTracor(
            this IServiceCollection serviceBuilder,
            bool addEnabledServices,
            Action<TracorOptions>? configureTracor,
            Action<TracorDataConvertOptions>? configureConvert,
            string tracorScopedFilterSection = "") {
        if (addEnabledServices) {
            return serviceBuilder.AddEnabledTracor(
                configureTracor,
                configureConvert,
                tracorScopedFilterSection);
        } else {
            return serviceBuilder.AddDisabledTracor(
                configureTracor,
                configureConvert,
                tracorScopedFilterSection);
        }
    }

    /// <summary>
    /// Adds a disabled Tracor services to the service collection. 
    /// Disabled Tracor does nothing - exists only so that the DI don't fail.
    /// </summary>
    /// <param name="serviceBuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ITracorBuilder AddDisabledTracor(
        this IServiceCollection serviceBuilder,
        Action<TracorOptions>? configureTracor,
        Action<TracorDataConvertOptions>? configureConvert,
        string tracorScopedFilterSection = "") {
        serviceBuilder.AddSingleton<TracorEmergencyLogging>();
        serviceBuilder.AddSingleton<ITracorCollectivePublisher, TracorCollectivePublisher>();
        serviceBuilder.AddSingleton<TracorDataRecordPool>();
        serviceBuilder.AddSingleton<TracorMemoryPoolManager>();
        serviceBuilder.AddSingleton<ITracorServiceSink, DisabledTracorServiceSink>();
        serviceBuilder.AddSingleton<ITracorDataConvertService>(TracorDataConvertService.Create);
        serviceBuilder.AddSingleton<LateTracorDataConvertService>();
        serviceBuilder.AddSingleton<DisabledTracorValidator>();
        serviceBuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<DisabledTracorValidator>());
        serviceBuilder.AddSingleton(typeof(LazyCreatedLogger<>));
        serviceBuilder.Add(ServiceDescriptor.Transient(
            typeof(ITracorSink<>), typeof(TracorSink<>)));

        serviceBuilder.AddTracorScopedFilter((builder) => {
            builder.AddTracorScopedFilterConfiguration(tracorScopedFilterSection);
        });

        if (configureTracor is { }) {
            var optionsBuilder = serviceBuilder.AddOptions<TracorOptions>();
            optionsBuilder.Configure(configureTracor);
        }
        if (configureConvert is { }) {
            var optionsBuilder = serviceBuilder.AddOptions<TracorDataConvertOptions>();
            optionsBuilder.Configure(configureConvert); 
        }

        return new TracorBuilder(serviceBuilder);
    }

    /// <summary>
    /// Adds the enabled Tracor services to the service collection with custom configuration.
    /// Enabled Tracor is designed for testing scenarios with full validation capabilities.
    /// </summary>
    /// <param name="serviceBuilder">The service collection to add services to.</param>
    /// <param name="configureConvert">An action to configure the Tracor validator options.</param>
    /// <returns>The service collection for method chaining.</returns>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ITracorBuilder AddEnabledTracor(
        this IServiceCollection serviceBuilder,
        Action<TracorOptions>? configureTracor = default,
        Action<TracorDataConvertOptions>? configureConvert = default,
        string tracorScopedFilterSection = "") {
        serviceBuilder.AddSingleton<TracorEmergencyLogging>();
        serviceBuilder.AddSingleton<ITracorCollectivePublisher, TracorCollectivePublisher>();
        serviceBuilder.AddSingleton<TracorDataRecordPool>();
        serviceBuilder.AddSingleton<TracorMemoryPoolManager>();
        serviceBuilder.AddSingleton<ITracorServiceSink, TracorServiceSink>();
        serviceBuilder.AddTransient<ITracorCollectiveSink>(
            static (sp) => sp.GetRequiredService<TracorValidator>());
        serviceBuilder.AddSingleton<ITracorDataConvertService>(TracorDataConvertService.Create);
        serviceBuilder.AddSingleton<LateTracorDataConvertService>();
        serviceBuilder.AddSingleton<TracorValidator>();
        serviceBuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<TracorValidator>());
        serviceBuilder.AddSingleton(typeof(LazyCreatedLogger<>));
        serviceBuilder.Add(ServiceDescriptor.Transient(
            typeof(ITracorSink<>), typeof(TracorSink<>)));

        serviceBuilder.AddTracorScopedFilter((builder) => {
            builder.AddTracorScopedFilterConfiguration(tracorScopedFilterSection);
        });

        {
            var optionsBuilder = serviceBuilder.AddOptions<TracorOptions>();
            if (configureTracor is { }) { optionsBuilder.Configure(configureTracor); }
        }
        {
            var optionsBuilder = serviceBuilder.AddOptions<TracorDataConvertOptions>();
            if (configureConvert is { }) { optionsBuilder.Configure(configureConvert); }
        }

        return new TracorBuilder(serviceBuilder);
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
                options.GetListActivitySourceResolver().Add(new InstrumentationBaseResolver<T>());
            });
        return servicebuilder;
    }

}