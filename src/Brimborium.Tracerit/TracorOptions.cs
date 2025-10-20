namespace Brimborium.Tracerit;

/// <summary>
/// Tracor Options
/// </summary>
public sealed class TracorOptions {
    /// <summary>
    /// Tracor is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Log to System.Console
    /// </summary>
    public bool IsEmergencyLogging { get; set; }

    /// <summary>
    /// Get or set ApplicationName
    /// </summary>
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