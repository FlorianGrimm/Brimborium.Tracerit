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

    private Func<IServiceProvider, CancellationToken>? _OnGetApplicationStopping;

    /// <summary>
    /// Important allows to retrieve the IHostApplicationLifetime.ApplicationStopping which is essential for periodical flush.
    /// So that at the end the buffer will be flushed.
    /// </summary>
    /// <example>
    /// fileTracorOptions.GetApplicationStopping = static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping
    /// </example>
    public Func<IServiceProvider, CancellationToken>? GetOnGetApplicationStopping() {
        return this._OnGetApplicationStopping;
    }

    public TracorOptions SetOnGetApplicationStopping(Func<IServiceProvider, CancellationToken>? value) {
        this._OnGetApplicationStopping = value;
        return this;
    }
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