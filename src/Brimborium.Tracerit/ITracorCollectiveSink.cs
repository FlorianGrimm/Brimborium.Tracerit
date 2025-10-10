namespace Brimborium.Tracerit;

public interface ITracorCollectiveSink {
    bool IsGeneralEnabled();
    bool IsEnabled();

    /// <summary>
    /// Processes a trace event with the specified caller and trace data.
    /// </summary>
    /// <param name="isPublic">allow the data to be persisted.</param>
    /// <param name="tracorData">The trace data to process.</param>
    void OnTrace(bool isPublic, ITracorData tracorData);
}
