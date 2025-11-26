namespace Brimborium.Tracerit.Diagnostics;

/// <summary>
/// Base interface for instrumentation that provides access to an ActivitySource.
/// </summary>
public interface IInstrumentation : IDisposable {
    /// <summary>
    /// Gets the ActivitySource for this instrumentation.
    /// </summary>
    ActivitySource? ActivitySource { get; }
}
