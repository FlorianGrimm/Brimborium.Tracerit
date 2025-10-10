namespace Brimborium.Tracerit;

/// <summary>
/// Represents the tracor sink.
/// </summary>
public interface ITracorSink {
    /// <summary>
    /// Checks if private tracor is enabled for this scope and level.
    /// </summary>
    /// <param name="scope">The scope</param>
    /// <param name="level">The level</param>
    /// <returns>enabled or not.</returns>
    bool IsPrivateEnabled(string? scope, LogLevel level);

    /// <summary>
    /// Checks if public tracor is enabled for this scope and level.
    /// </summary>
    /// <param name="scope">The relative scope</param>
    /// <param name="level">The level</param>
    /// <returns>enabled or not.</returns>
    bool IsPublicEnabled(string? scope, LogLevel level);

    /// <summary>
    /// Private Trace - no level check
    /// </summary>
    /// <typeparam name="T">The value</typeparam>
    /// <param name="scope">The relative scope</param>
    /// <param name="level">The log level</param>
    /// <param name="message">The message</param>
    /// <param name="value">The trace value</param>
    void TracePrivate<T>(string? scope, LogLevel level, string message, T value);

    /// <summary>
    /// Public Trace - no level check
    /// </summary>
    /// <typeparam name="T">The value</typeparam>
    /// <param name="scope">The relative scope</param>
    /// <param name="level">The log level</param>
    /// <param name="message">The message</param>
    /// <param name="value">The trace value</param>
    void TracePublic<T>(string? scope, LogLevel level, string message, T value);
}

public interface ITracorSink<out TCategoryName> : ITracorSink {
}