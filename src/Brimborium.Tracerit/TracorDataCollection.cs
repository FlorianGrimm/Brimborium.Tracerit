namespace Brimborium.Tracerit;

/// <summary>
/// Represents a collection of <see cref="TracorDataRecord"/> items for trace data management.
/// </summary>
public sealed class TracorDataCollection {
    /// <summary>
    /// Gets the list of <see cref="TracorDataRecord"/> items contained in this collection.
    /// </summary>
    public List<TracorDataRecord> ListData { get; } = [];

    /// <summary>
    /// Converts the collection of <see cref="TracorDataRecord"/> items to a list of <see cref="TracorIdentitfierData"/> items.
    /// </summary>
    /// <returns>
    /// A list of <see cref="TracorIdentitfierData"/> representing the identifier data for each record in the collection.
    /// </returns>
    public List<TracorIdentitfierData> ToListTracorIdentitfierData() {
        List<TracorIdentitfierData> result = new(this.ListData.Count);
        foreach (var item in this.ListData) {
            result.Add(item.ToTracorIdentitfierData());
        }
        return result;
    }
}
