// MIT - Florian Grimm

namespace Brimborium.Tracerit.Server;

/// <summary>
/// Collector interface for storing and retrieving trace data records.
/// </summary>
public interface ITracorServerCollectorWrite {
    /// <summary>
    /// add the data to the collection
    /// </summary>
    /// <param name="tracorDataRecord"></param>
    void Push(TracorDataRecord tracorDataRecord);
}

public interface ITracorServerCollectorReadAndWrite : ITracorServerCollectorWrite {
    /// <summary>
    /// Get the pushed data.
    /// </summary>
    /// <param name="name">empty or named</param>
    /// <returns></returns>
    TracorDataCollection GetListTracorDataRecord(string? name);
}
