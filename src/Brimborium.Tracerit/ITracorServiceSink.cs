namespace Brimborium.Tracerit;

/// <summary>
/// Represents the core tracing interface for capturing and processing trace data.
/// </summary>
public interface ITracorServiceSink {
    // so the DI has to resolve only ITracorServiceSink and not both
    /// <summary>
    /// Get the factory
    /// </summary>
    /// <returns>the factory</returns>
    ITracorScopedFilterFactory GetTracorScopedFilterFactory();

    /// <summary>
    /// Determines if tracing is generally enabled at the configuration level.
    /// </summary>
    /// <returns>True if tracing is generally enabled; otherwise, false.</returns>
    bool IsGeneralEnabled();

    /// <summary>
    /// Determines if tracing is currently enabled and active for processing.
    /// </summary>
    /// <returns>True if tracing is currently enabled; otherwise, false.</returns>
    bool IsCurrentlyEnabled();

    bool IsPrivateEnabled(string scope, LogLevel logLevel);

    bool IsPublicEnabled(string scope, LogLevel logLevel);

    void TracePrivate<T>(string scope, LogLevel level, string message, T value);

    void TracePublic<T>(string scope, LogLevel level, string message, T value);
}

