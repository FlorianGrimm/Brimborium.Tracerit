namespace Brimborium.Tracerit;

/// <summary>
/// Represents the tracor sink.
/// </summary>
public interface ITracorSink {
    bool IsPrivateEnabled(string scope, LogLevel level);

    bool IsPublicEnabled(string scope, LogLevel level);

    void TracePrivate<T>(string scope, LogLevel level, string message, T value);

    void TracePublic<T>(string scope, LogLevel level, string message, T value);
}

/// <summary>
/// Represents the core tracing interface for capturing and processing trace data.
/// </summary>
public interface ITracorServiceSink {
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

