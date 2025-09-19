namespace Brimborium.Tracerit;

public sealed class TracorDataCollection {
    public List<TracorDataRecord> ListData { get; } = [];

    public List<TracorIdentitfierData> ToListTracorIdentitfierData() {
        List<TracorIdentitfierData> result = new(this.ListData.Count);
        foreach (var item in this.ListData) {
            result.Add(item.ToTracorIdentitfierData());
        }
        return result;
    }
}
