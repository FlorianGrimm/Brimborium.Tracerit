// MIT - Florian Grimm

namespace Brimborium.Tracerit.Server;

public sealed class TracorCollectorService : ITracorCollector {
    private readonly Lock _Lock = new();
    private readonly List<TracorDataRecord> _ListTracorDataRecord = new(1024);
    public TracorCollectorService() {
    }

    public List<TracorDataRecord> GetListTracorDataRecord() {
        List<TracorDataRecord> result;
        using (this._Lock.EnterScope()) {
            result = new List<TracorDataRecord>(this._ListTracorDataRecord);
        }
        return result;
    }

    public void Push(TracorDataRecord tracorDataRecord) {
        using (this._Lock.EnterScope()) {
            if (1024 <= this._ListTracorDataRecord.Count) { 
                this._ListTracorDataRecord.RemoveRange(0, 512);
                this._ListTracorDataRecord.Add(tracorDataRecord);
            }
        }
    }
    
}

public interface ITracorCollector {
    void Push(TracorDataRecord tracorDataRecord);
    List<TracorDataRecord> GetListTracorDataRecord();
}
