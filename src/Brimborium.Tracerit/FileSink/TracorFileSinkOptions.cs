namespace Brimborium.Tracerit.FileSink;

/// <summary>
/// Use GetApplicationStopping or FlushAsync to prevent loose log entries.
/// </summary>
public sealed class TracorFileSinkOptions : TracorBulkSinkOptions {

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
    /// None, brotli, gzip
    /// </summary>
    public string? Compression { get; set; }

    public HashSet<string> ListIgnoreProperty { get; } = [];

    /// <summary>
    /// If enabled periodical will checked if the old files should be deleted.
    /// </summary>
    public bool CleanupEnabled { get; set; }

    /// <summary>
    /// If CleanupEnabled; this defines the timespan after the log files considered to be deleted.
    /// </summary>
    public TimeSpan CleanupPeriod { get; set; } = TimeSpan.FromDays(31);
}