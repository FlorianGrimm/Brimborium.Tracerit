namespace Brimborium.Tracerit.Logger;

/// <summary>
/// Configuration options for the Tracor logger provider.
/// </summary>
public sealed class TracorLoggerOptions {
    /// <summary>
    /// Gets or sets the minimum log level for the Tracor logger.
    /// When null, all log levels are enabled.
    /// </summary>
    public LogLevel? LogLevel { get; set; }
}