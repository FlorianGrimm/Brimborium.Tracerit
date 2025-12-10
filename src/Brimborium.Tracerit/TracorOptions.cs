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

    /// <summary>
    /// Get or set ApplicationVersion
    /// </summary>
    public string? ApplicationVersion { get; set; }

    /// <summary>
    /// Important allows to retrieve the IHostApplicationLifetime.ApplicationStopping which is essential for periodical flush.
    /// So that at the end the buffer will be flushed.
    /// </summary>
    /// <example>
    /// tracorOptions.SetOnGetApplicationStopping(static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
    /// </example>
    public TracorOptions SetOnGetApplicationStopping(Func<IServiceProvider, CancellationToken>? value) {
        this._OnGetApplicationStopping = value;
        return this;
    }

    private Func<IServiceProvider, CancellationToken>? _OnGetApplicationStopping;

    public Func<IServiceProvider, CancellationToken>? GetOnGetApplicationStopping() {
        return this._OnGetApplicationStopping;
    }
}

public static class TracorOptionsExtension {

    /// <summary>
    /// Bind the configuration.Section(Tracor) to option.
    /// </summary>
    /// <remarks>
    /// "Tracor" is the default path.
    /// This allows the source generator to kick in.
    /// </remarks>
    /// <param name="configuration">the root configuration</param>
    /// <param name="options">bind targer</param>
    /// <returns>options</returns>
    public static TracorOptions BindTracorOptionsDefault(
        this IConfigurationRoot configuration,
        TracorOptions options
        ) {
        TracorBuilderExtension.GetConfigurationTracorSection(configuration).Bind(options);
        return options;
    }

    /// <summary>
    /// Bind this configuration to option.
    /// </summary>
    /// <remarks>
    /// This allows the source generator to kick in.
    /// </remarks>
    /// <param name="configuration">the root configuration</param>
    /// <param name="options">bind targer</param>
    /// <returns>options</returns>
    public static TracorOptions BindTracorOptionsCustom(
        this IConfiguration configuration,
        TracorOptions options
        ) {
        configuration.Bind(options);
        return options;
    }

    public static string GetApplicationName(this TracorOptions that) {
        if (that.ApplicationName is not { } applicationName) {
            if (System.Reflection.Assembly.GetEntryAssembly() is { } assembly) {
                applicationName = assembly.GetName().Name;
            } else { 
                applicationName = null;
            }
            return that.ApplicationName = (applicationName ?? "Application");
        } else {
            return applicationName;
        }
    }
}