namespace Brimborium.Tracerit;

public static class TracorServiceBuilderExtension {
    public static IServiceCollection AddRuntimeTracor(this IServiceCollection servicebuilder) {
        servicebuilder.AddSingleton<ITracor, RuntimeTracor>();
        servicebuilder.AddSingleton<RuntimeTracorValidator>();
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<RuntimeTracorValidator>());
        return servicebuilder;
    }
    public static IServiceCollection AddTesttimeTracor(this IServiceCollection servicebuilder) {
        return servicebuilder.AddTesttimeTracor(configure: (options) => { });
    }
    public static IServiceCollection AddTesttimeTracor(
        this IServiceCollection servicebuilder,
        Action<TracorValidatorOptions> configure) {
        servicebuilder.AddSingleton<ITracor, TesttimeTracor>();
        servicebuilder.AddSingleton<TesttimeTracorValidator>();
        servicebuilder.AddSingleton<ITracorValidator>(
            static (serviceProvider) => serviceProvider.GetRequiredService<TesttimeTracorValidator>());
        var optionsBuilder = servicebuilder.AddOptions<TracorValidatorOptions>();
        if (configure is { }) { optionsBuilder.Configure(configure); }
        return servicebuilder;
    }
}
