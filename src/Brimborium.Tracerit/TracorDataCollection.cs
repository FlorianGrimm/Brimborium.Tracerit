using System.Runtime.InteropServices;

namespace Brimborium.Tracerit;

/// <summary>
/// Represents a collection of <see cref="TracorDataRecord"/> items for trace data management.
/// 
/// <see cref="Add(ITracorData)"/>,
/// <see cref="AddRange(IEnumerable{ITracorData})"/> and
/// <see cref="Dispose"/>
/// respect the <see cref="IReferenceCountObject"/>.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class TracorDataCollection : IDisposable {
    public TracorDataCollection() {
    }

    public TracorDataCollection(IEnumerable<ITracorData> src) {
        this.AddRange(src);
    }

    public void Add(ITracorData src) {
        if (src is IReferenceCountObject referenceCountObject) {
            referenceCountObject.IncrementReferenceCount();
        }
        this.ListData.Add(src);
    }

    public void AddRangeSpan<T>(Span<T> src)
        where T: class, ITracorData {
        this.ListData.EnsureCapacity(
            this.ListData.Count + src.Length);
        foreach (var srcItem in src) {
            if (srcItem is IReferenceCountObject referenceCountObject) {
                referenceCountObject.IncrementReferenceCount();
            }
            this.ListData.Add(srcItem);
        }
    }

    public void AddRange(IEnumerable<ITracorData> src) {
        this.ListData.EnsureCapacity(
            this.ListData.Count + src.Count());
        foreach (var srcItem in src) {
            if (srcItem is IReferenceCountObject referenceCountObject) {
                referenceCountObject.IncrementReferenceCount();
            }
            this.ListData.Add(srcItem);
        }
    }

    public void Dispose() {
        foreach (var item in this.ListData) {
            if (item is IReferenceCountObject referenceCountObject) {
                referenceCountObject.Dispose();
            }
        }
        this.ListData.Clear();
    }

    /// <summary>
    /// Gets the list of <see cref="TracorDataRecord"/> items contained in this collection.
    /// </summary>
    public List<ITracorData> ListData { get; } = [];

    private string GetDebuggerDisplay() {
        return $"Count: {this.ListData.Count}";
    }
}

public sealed class TracorDataRecordCollection : IDisposable {
    public TracorDataRecordCollection() {
    }

    public TracorDataRecordCollection(IEnumerable<TracorDataRecord> src) {
        this.ListData.AddRange(src);
    }
    /// <summary>
    /// Gets the list of <see cref="TracorDataRecord"/> items contained in this collection.
    /// </summary>
    public List<TracorDataRecord> ListData { get; } = [];

    public void Dispose() {
        foreach (var item in this.ListData) {
            item.Dispose();
        }
        this.ListData.Clear();
    }
}
