namespace Brimborium.Tracerit.HttpSink;

/// <summary>
/// Use GetApplicationStopping or FlushAsync to prevent loose log entries.
/// </summary>
public sealed class TracorHttpSinkOptions : TracorBulkSinkOptions {
    /// <summary>
    /// The target Endpoint - e.g https://localhost:1439.
    /// </summary>
    public string? TargetUrl { get; set; }

    /// <summary>
    /// The target Endpoint.
    /// </summary>
    /// <remarks>
    /// The purpuse is for testing - so you don't override other configurations.
    /// </remarks>
    public string? TestingTargetUrl { get; set; }

    /// <summary>
    /// A list of target endpoints.
    /// </summary>
    public List<string> ListTargetUrl { get; set; } = [];

    /// <summary>
    /// None, brotli, gzip
    /// </summary>
    public string? Compression { get; set; }
}