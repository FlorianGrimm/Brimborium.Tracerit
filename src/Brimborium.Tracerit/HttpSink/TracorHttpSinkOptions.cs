namespace Brimborium.Tracerit.HttpSink;

/// <summary>
/// Use GetApplicationStopping or FlushAsync to prevent loose log entries.
/// </summary>
public sealed class TracorHttpSinkOptions : TracorBulkSinkOptions {
    /// <summary>
    /// The target Endpoint.
    /// </summary>
    public string? TargetUrl { get; set; }

    /// <summary>
    /// None, brotli, gzip
    /// </summary>
    public string? Compression { get; set; }
}