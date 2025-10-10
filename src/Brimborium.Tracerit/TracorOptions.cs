namespace Brimborium.Tracerit;

public class TracorOptions {
    public bool IsEnabled { get; set; }

    public string? ApplicationName { get; set; }
}

public static class TracorOptionsExtension {

    public static TracorOptions BindTracorOptionsDefault(
        this IConfigurationRoot configuration,
        TracorOptions options
        ) {
        TracorBuilderExtension.GetConfigurationTracorSection(configuration).Bind(options);
        return options;
    }

    public static TracorOptions BindTracorOptionsCustom(
        this IConfiguration configuration,
        TracorOptions options
        ) {
        configuration.Bind(options);
        return options;
    }
}