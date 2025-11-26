namespace Brimborium.Tracerit;

/// <summary>
/// Collective sink interface that receives and processes all trace events from multiple sources.
/// </summary>
public interface ITracorCollectiveSink {
    /// <summary>
    /// Determines if tracing is generally enabled at the configuration level.
    /// </summary>
    /// <returns>True if tracing is generally enabled; otherwise, false.</returns>
    bool IsGeneralEnabled();

    /// <summary>
    /// Determines if tracing is currently enabled and active for processing.
    /// </summary>
    /// <returns>True if tracing is currently enabled; otherwise, false.</returns>
    bool IsEnabled();

    /// <summary>
    /// Processes a trace event with the specified caller and trace data.
    /// </summary>
    /// <param name="isPublic">allow the data to be persisted.</param>
    /// <param name="tracorData">The trace data to process.</param>
    void OnTrace(bool isPublic, ITracorData tracorData);
}
