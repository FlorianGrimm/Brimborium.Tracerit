// MIT - Florian Grimm

namespace Brimborium.Tracerit.Server;

/// <summary>
/// Configuration options for the <see cref="TracorCollectorService"/>.
/// </summary>
public sealed class TracorCollectorOptions {
    /// <summary>
    /// Gets or sets the maximum capacity of trace records to store. Default is 2048.
    /// </summary>
    public int Capacity { get; set; } = 2048;
}

/// <summary>
/// Collector service for storing and retrieving trace data records.
/// Maintains a bounded queue of trace records with support for partial retrieval by named clients.
/// </summary>
public sealed class TracorCollectorService : ITracorCollector {
    private readonly Lock _Lock = new();
    private readonly int _Capacity;

    private readonly Queue<TracorDataRecord> _QueueTracorDataRecord;
    private readonly Dictionary<string, PartialCount> _PartialCountByName;

    private class PartialCount {
        public int Missed;
    }

    public TracorCollectorService(IOptions<TracorCollectorOptions> options) {
        var optionsValue = options.Value;
        this._Capacity = optionsValue.Capacity < 128 ? 128 : optionsValue.Capacity;
        this._QueueTracorDataRecord = new Queue<TracorDataRecord>();
        this._QueueTracorDataRecord.EnsureCapacity(this._Capacity);
        this._PartialCountByName = new();
    }

    public void Push(TracorDataRecord tracorDataRecord) {
        using (this._Lock.EnterScope()) {
            while (this._Capacity <= this._QueueTracorDataRecord.Count) {
                if (this._QueueTracorDataRecord.Dequeue() is ReferenceCountObject referenceCountObject) {
                    referenceCountObject.Dispose();
                }
            }

            {
                if (tracorDataRecord is ReferenceCountObject referenceCountObject) {
                    referenceCountObject.IncrementReferenceCount();
                }
            }

            this._QueueTracorDataRecord.Enqueue(tracorDataRecord);

            if (0 < this._PartialCountByName.Count) {
                var capacity = this._Capacity;
                string? keyToRemove = null;
                foreach (var keyValue in this._PartialCountByName) {
                    var missed = ++keyValue.Value.Missed;
                    if (capacity <= missed) {
                        // their may be more but the next Push will delete the next
                        keyToRemove = keyValue.Key;
                    }
                }
                if (keyToRemove != null) {
                    this._PartialCountByName.Remove(keyToRemove);
                }
            }
        }
    }

    public TracorDataCollection GetListTracorDataRecord(string? name) {
        TracorDataCollection result = new();
        using (this._Lock.EnterScope()) {
            if (name is { Length: > 0 }) {
                if (this._PartialCountByName.TryGetValue(name, out var partialCount)) {
                    var capacity = this._Capacity;
                    if (0 == partialCount.Missed) {
                        return result;
                    }
                    if (partialCount.Missed < capacity) {
                        var listTracorDataRecord = this._QueueTracorDataRecord.ToArray();

                        int missed = partialCount.Missed;
                        int length = listTracorDataRecord.Length;
                        int indexLow = (length < missed) ? 0 : (length - missed);
                        result.ListData.EnsureCapacity(missed);
                        result.AddRangeSpan(listTracorDataRecord.AsSpan(indexLow));
                        partialCount.Missed = 0;
                        if (!(result.ListData.Count == missed)) {
                            throw new InvalidOperationException($"{result.ListData.Count} == {missed}");
                        }
                        return result;
                    } else {
                        partialCount.Missed = 0;
                    }
                } else {
                    this._PartialCountByName.Add(name, new PartialCount());
                }
            }

            {
                var listTracorDataRecord = this._QueueTracorDataRecord.ToArray();

                result.AddRangeSpan(listTracorDataRecord.AsSpan());
                return result;
            }
        }
    }
}
