using System.Runtime.InteropServices;

namespace Brimborium.Tracerit;

/// <summary>
/// Represents a collection of <see cref="ITracorData"/> items for trace data management.
/// Methods <see cref="Add(ITracorData)"/>, <see cref="AddRange(IEnumerable{ITracorData})"/>
/// and <see cref="Dispose"/> properly manage reference counting via <see cref="IReferenceCountObject"/>.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class TracorDataCollection : IDisposable {
    /// <summary>
    /// Initializes a new empty instance of the <see cref="TracorDataCollection"/> class.
    /// </summary>
    public TracorDataCollection() {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorDataCollection"/> class with items from the source enumerable.
    /// </summary>
    /// <param name="src">The source enumerable of trace data items to add.</param>
    public TracorDataCollection(IEnumerable<ITracorData> src) {
        this.AddRange(src);
    }

    /// <summary>
    /// Adds a trace data item to the collection and increments its reference count if applicable.
    /// </summary>
    /// <param name="src">The trace data item to add.</param>
    public void Add(ITracorData src) {
        if (src is IReferenceCountObject referenceCountObject) {
            referenceCountObject.IncrementReferenceCount();
        }
        this.ListData.Add(src);
    }

    /// <summary>
    /// Adds a span of trace data items to the collection, incrementing reference counts as applicable.
    /// </summary>
    /// <typeparam name="T">The type of trace data items, must implement <see cref="ITracorData"/>.</typeparam>
    /// <param name="src">The span of trace data items to add.</param>
    public void AddRangeSpan<T>(Span<T> src)
        where T : class, ITracorData {
        this.ListData.EnsureCapacity(
            this.ListData.Count + src.Length);
        foreach (var srcItem in src) {
            if (srcItem is IReferenceCountObject referenceCountObject) {
                referenceCountObject.IncrementReferenceCount();
            }
            this.ListData.Add(srcItem);
        }
    }

    /// <summary>
    /// Adds a range of trace data items to the collection, incrementing reference counts as applicable.
    /// </summary>
    /// <param name="src">The enumerable of trace data items to add.</param>
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

    /// <summary>
    /// Disposes all items in the collection by decrementing their reference counts and clears the list.
    /// </summary>
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

/// <summary>
/// Represents a strongly-typed collection of <see cref="TracorDataRecord"/> items for trace data management.
/// Properly manages reference counting through <see cref="IReferenceCountObject"/> on disposal.
/// </summary>
public sealed class TracorDataRecordCollection : IDisposable {
    /// <summary>
    /// Initializes a new empty instance of the <see cref="TracorDataRecordCollection"/> class.
    /// </summary>
    public TracorDataRecordCollection() {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorDataRecordCollection"/> class with items from the source enumerable.
    /// </summary>
    /// <param name="src">The source enumerable of trace data records to add.</param>
    public TracorDataRecordCollection(IEnumerable<TracorDataRecord> src) {
        this.ListData.AddRange(src);
    }

    /// <summary>
    /// Gets the list of <see cref="TracorDataRecord"/> items contained in this collection.
    /// </summary>
    public List<TracorDataRecord> ListData { get; } = [];

    /// <summary>
    /// Disposes all items in the collection by decrementing their reference counts and clears the list.
    /// </summary>
    public void Dispose() {
        foreach (var item in this.ListData) {
            item.Dispose();
        }
        this.ListData.Clear();
    }
}
