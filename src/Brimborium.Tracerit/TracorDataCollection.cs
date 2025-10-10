namespace Brimborium.Tracerit;

/// <summary>
/// Represents a collection of <see cref="TracorDataRecord"/> items for trace data management.
/// </summary>
public sealed class TracorDataCollection {
    public TracorDataCollection() {            
    }

    public TracorDataCollection(IEnumerable<ITracorData> src) {            
        this.ListData.AddRange(src);
    }

    /// <summary>
    /// Gets the list of <see cref="TracorDataRecord"/> items contained in this collection.
    /// </summary>
    public List<ITracorData> ListData { get; } = [];
}

public sealed class TracorDataRecordCollection {
    public TracorDataRecordCollection() {
    }

    public TracorDataRecordCollection(IEnumerable<TracorDataRecord> src) {
        this.ListData.AddRange(src);
    }
    /// <summary>
    /// Gets the list of <see cref="TracorDataRecord"/> items contained in this collection.
    /// </summary>
    public List<TracorDataRecord> ListData { get; } = [];
}
