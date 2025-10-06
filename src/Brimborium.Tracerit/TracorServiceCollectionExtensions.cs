namespace Microsoft.Extensions.DependencyInjection;

public static partial class TracorServiceCollectionExtensions {
    internal const string RequiresDynamicCodeMessage = "Binding TOptions to configuration values may require generating dynamic code at runtime.";
    internal const string TrimmingRequiresUnreferencedCodeMessage = "TOptions's dependent types may have their members trimmed. Ensure all required members are preserved.";

    /// <summary>
    /// Adds runtime or testtime Tracor services to the service collection.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ITracorBuilder AddTracor(
        this IServiceCollection servicebuilder,
        bool addTestTimeServices,
        Action<TracorDataConvertOptions>? configure = default,
        string tracorScopedFilterSection = "") {
        if (addTestTimeServices) {
            return servicebuilder.AddTesttimeTracor(configure, tracorScopedFilterSection);
        } else {
            return servicebuilder.AddRuntimeTracor(configure, tracorScopedFilterSection);
        }
    }

    /// <summary>
    /// Adds runtime Tracor services to the service collection. Runtime Tracor is optimized for production use with minimal overhead.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ITracorBuilder AddRuntimeTracor(
        this IServiceCollection servicebuilder,
        Action<TracorDataConvertOptions>? configure = default,
        string tracorScopedFilterSection = "") {
        servicebuilder.AddSingleton<ITracorCollectivePublisher, TracorCollectivePublisher>();
        servicebuilder.AddSingleton<TracorDataRecordPool>(TracorDataRecordPool.Create);
        servicebuilder.AddSingleton<ITracorServiceSink, RuntimeTracorServiceSink>();
        servicebuilder.AddSingleton<ITracorDataConvertService, TracorDataConvertService>();
        servicebuilder.AddSingleton<RuntimeTracorValidator>();
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<RuntimeTracorValidator>());
        servicebuilder.AddSingleton(typeof(LazyCreatedLogger<>));

        servicebuilder.AddTracorScopedFilter((builder) => {
            builder.AddTracorScopedFilterConfiguration(tracorScopedFilterSection);
        });

        var optionsBuilder = servicebuilder.AddOptions<TracorDataConvertOptions>();
        if (configure is { }) { optionsBuilder.Configure(configure); }

        return new TracorBuilder(servicebuilder);
    }

    /// <summary>
    /// Adds test-time Tracor services to the service collection with custom configuration.
    /// Test-time Tracor is designed for testing scenarios with full validation capabilities.
    /// </summary>
    /// <param name="servicebuilder">The service collection to add services to.</param>
    /// <param name="configure">An action to configure the Tracor validator options.</param>
    /// <returns>The service collection for method chaining.</returns>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ITracorBuilder AddTesttimeTracor(
        this IServiceCollection servicebuilder,
        Action<TracorDataConvertOptions>? configure = default,
        string tracorScopedFilterSection = "") {
        servicebuilder.AddSingleton<ITracorCollectivePublisher, TracorCollectivePublisher>();
        servicebuilder.AddSingleton<ActivityTracorDataPool>(ActivityTracorDataPool.Create);
        servicebuilder.AddSingleton<TracorDataRecordPool>(TracorDataRecordPool.Create);
        servicebuilder.AddSingleton<ITracorServiceSink, TesttimeTracorServiceSink>();
        servicebuilder.AddTransient<ITracorCollectiveSink>(static(sp)=>sp.GetRequiredService<TesttimeTracorValidator>());
        servicebuilder.AddSingleton<ITracorDataConvertService, TracorDataConvertService>();
        servicebuilder.AddSingleton<TesttimeTracorValidator>(TesttimeTracorValidator.Create);
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<TesttimeTracorValidator>());
        servicebuilder.AddSingleton(typeof(LazyCreatedLogger<>));

        servicebuilder.AddTracorScopedFilter((builder) => {
            builder.AddTracorScopedFilterConfiguration(tracorScopedFilterSection);
        });

        var optionsBuilder = servicebuilder.AddOptions<TracorDataConvertOptions>();
        if (configure is { }) { optionsBuilder.Configure(configure); }

        return new TracorBuilder(servicebuilder);
    }
}