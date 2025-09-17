namespace Brimborium.Tracerit.Service;

/// <summary>
/// Runtime implementation of <see cref="ITracor"/> that disables tracing for production scenarios.
/// This implementation provides a no-op tracer that disposes of disposable values but performs no actual tracing.
/// </summary>
internal sealed class RuntimeTracor : ITracor {
    /// <summary>
    /// Determines if tracing is generally enabled at the configuration level.
    /// </summary>
    /// <returns>Always returns false for runtime scenarios.</returns>
    public bool IsGeneralEnabled() => false;

    /// <summary>
    /// Determines if tracing is currently enabled and active for processing.
    /// </summary>
    /// <returns>Always returns false for runtime scenarios.</returns>
    public bool IsCurrentlyEnabled() => false;

    /// <summary>
    /// Traces a value with the specified caller identifier. In runtime mode, this only disposes disposable values.
    /// </summary>
    /// <typeparam name="T">The type of the value being traced.</typeparam>
    /// <param name="callee">The identifier of the caller or trace point.</param>
    /// <param name="value">The value to be traced.</param>
    public void Trace<T>(TracorIdentitfier callee, T value) {
        // this is should not be called, but anyway...
        if (value is IDisposable valueDisposable) {
            valueDisposable.Dispose();
        }
    }
}
