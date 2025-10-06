namespace Brimborium.Tracerit;

public interface ITracorCollectiveSink {
    bool IsEnabled();

    /// <summary>
    /// Processes a trace event with the specified caller and trace data.
    /// </summary>
    /// <param name="isPublic">allow the data to be persisted.</param>
    /// <param name="callee">The identifier of the caller or trace point.</param>
    /// <param name="tracorData">The trace data to process.</param>
    void OnTrace(bool isPublic, TracorIdentitfier callee, ITracorData tracorData);
}
