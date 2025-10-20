namespace Brimborium.Tracerit.FileSink;

/// <summary>
/// Use GetApplicationStopping or FlushAsync to prevent loose log entries.
/// </summary>
public sealed class FileTracorOptions {
    /// <summary>
    /// (BaseDirectory or GetBaseDirectory) + Directory + FileName
    /// </summary>
    public string? BaseDirectory { get; set; }

    /// <summary>
    /// (BaseDirectory or GetBaseDirectory) + Directory + FileName
    /// </summary>
    public Func<string?>? GetBaseDirectory { get; set; }

    /// <summary>
    /// (BaseDirectory or GetBaseDirectory) + Directory + FileName
    /// </summary>
    public string? Directory { get; set; } = "Logs";

    /// <summary>
    /// (BaseDirectory or GetBaseDirectory) + Directory + FileName
    /// Replacements:
    ///  - {ApplicationName} from TracorOptions.ApplicationName or application assembly name.
    ///  - {TimeStamp} from start period formatted yyyy-MM-dd-HH-mm
    /// </summary>
    public string? FileName { get; set; } // = "log-{ApplicationName}_{TimeStamp}.jsonl";

    /// <summary>
    /// The period - the time spam for one log file.
    /// </summary>
    public TimeSpan Period { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    /// The period after the buffer will be flushed.
    /// </summary>
    public TimeSpan FlushPeriod { get; set; } = TimeSpan.FromSeconds(1);

    public string? Compression { get; set; }

    /// <summary>
    /// If enabled periodical will checked if the old files should be deleted.
    /// </summary>
    public bool CleanupEnabled { get; set; }

    /// <summary>
    /// If CleanupEnabled; this defines the timespan after the log files considured to be deleted.
    /// </summary>
    public TimeSpan CleanupPeriod { get; set; } = TimeSpan.FromDays(31);

    /// <summary>
    /// Important allows to retrive the IHostApplicationLifetime.ApplicationStopping which is essential for periodical flush.
    /// So that at the end the buffer will be flushed.
    /// </summary>
    /// <example>
    /// fileTracorOptions.GetApplicationStopping = static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping
    /// </example>
    public Func<IServiceProvider, CancellationToken>? GetApplicationStopping { get; set; }
}