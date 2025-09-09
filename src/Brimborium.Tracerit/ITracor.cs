namespace Brimborium.Tracerit;

/// <summary>
/// Represents the core tracing interface for capturing and processing trace data.
/// </summary>
public interface ITracor {
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
    /// Traces a value with the specified caller identifier.
    /// </summary>
    /// <typeparam name="T">The type of the value being traced.</typeparam>
    /// <param name="callee">The identifier of the caller or trace point.</param>
    /// <param name="Value">The value to be traced.</param>
    void Trace<T>(TracorIdentitfier callee, T Value);
}
