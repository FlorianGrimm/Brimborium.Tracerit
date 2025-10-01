namespace Brimborium.Tracerit;

/// <summary>
/// Represents the tracor sink.
/// </summary>
public interface ITracorSink {
    /// <summary>
    /// Traces a value with the specified caller identifier.
    /// </summary>
    /// <typeparam name="T">The type of the value being traced.</typeparam>
    /// <param name="callee">The identifier of the caller or trace point.</param>
    /// <param name="value">The value to be traced.</param>
    void TracePrivate<T>(TracorIdentitfier callee, LogLevel level, T value);

    /// <summary>
    /// Traces a value with the specified caller identifier.
    /// </summary>
    /// <typeparam name="T">The type of the value being traced.</typeparam>
    /// <param name="callee">The identifier of the caller or trace point.</param>
    /// <param name="value">The value to be traced.</param>
    void TracePublic<T>(TracorIdentitfier callee, LogLevel level, T value);
}

/// <summary>
/// Represents the core tracing interface for capturing and processing trace data.
/// </summary>
public interface ITracor : ITracorSink {
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

    bool IsPrivateEnabled(LogLevel logLevel);

    bool IsPublicEnabled(LogLevel logLevel);

    TracorLevel GetPrivateTracorEnabled(LogLevel logLevel);

    TracorLevel GetPublicTracorEnabled(LogLevel logLevel);
}

public readonly record struct TracorLevel(bool enabled, ITracorSink tracorSink);