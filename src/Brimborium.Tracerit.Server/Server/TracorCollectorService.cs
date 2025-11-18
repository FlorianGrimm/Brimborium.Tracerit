// MIT - Florian Grimm

using Brimborium.Tracerit.Utility;

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
            while (128 <= this._ListTracorDataRecord.Count) {
                var tracorDataRecordToRemove=this._ListTracorDataRecord[0];
                this._ListTracorDataRecord.RemoveAt(0);
                if (tracorDataRecordToRemove is ReferenceCountObject referenceCountObject) {
                    referenceCountObject.Dispose();
                }
            }
            {
                if (tracorDataRecord is ReferenceCountObject referenceCountObject) {
                    referenceCountObject.IncrementReferenceCount();
                }
            }
            this._ListTracorDataRecord.Add(tracorDataRecord);
        }
    }
}

public interface ITracorCollector {
    void Push(TracorDataRecord tracorDataRecord);
    List<TracorDataRecord> GetListTracorDataRecord();
}
