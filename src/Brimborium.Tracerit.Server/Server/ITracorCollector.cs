// MIT - Florian Grimm

namespace Brimborium.Tracerit.Server;

/// <summary>
/// 
/// </summary>
public interface ITracorCollector {
    /// <summary>
    /// add the data to the collection
    /// </summary>
    /// <param name="tracorDataRecord"></param>
    void Push(TracorDataRecord tracorDataRecord);

    /// <summary>
    /// Get the pushed data.
    /// </summary>
    /// <param name="name">empty or named</param>
    /// <returns></returns>
    TracorDataCollection GetListTracorDataRecord(string? name);
}
