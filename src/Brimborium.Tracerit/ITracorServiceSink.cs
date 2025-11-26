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

    /// <summary>
    /// Checks if private tracing is enabled for the specified scope and log level.
    /// </summary>
    /// <param name="scope">The tracing scope.</param>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>True if private tracing is enabled; otherwise, false.</returns>
    bool IsPrivateEnabled(string scope, LogLevel logLevel);

    /// <summary>
    /// Checks if public tracing is enabled for the specified scope and log level.
    /// </summary>
    /// <param name="scope">The tracing scope.</param>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>True if public tracing is enabled; otherwise, false.</returns>
    bool IsPublicEnabled(string scope, LogLevel logLevel);

    /// <summary>
    /// Traces a private event with the specified scope, level, message, and value.
    /// </summary>
    /// <typeparam name="T">The type of the trace value.</typeparam>
    /// <param name="scope">The tracing scope.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The trace message.</param>
    /// <param name="value">The value to trace.</param>
    void TracePrivate<T>(string scope, LogLevel level, string message, T value);

    /// <summary>
    /// Traces a public event with the specified scope, level, message, and value.
    /// </summary>
    /// <typeparam name="T">The type of the trace value.</typeparam>
    /// <param name="scope">The tracing scope.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The trace message.</param>
    /// <param name="value">The value to trace.</param>
    void TracePublic<T>(string scope, LogLevel level, string message, T value);
}

