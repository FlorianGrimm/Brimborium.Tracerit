namespace Brimborium.Tracerit;

public sealed class TracorDataRecord : ITracorData {
    public TracorDataRecord() { }

    public TracorIdentitfier? TracorIdentitfier { get; set; }

    public List<TracorDataProperty> ListProperty { get; } = [];

    public TracorIdentitfierData ToTracorIdentitfierData()
        => new TracorIdentitfierData(
            this.TracorIdentitfier ?? new(string.Empty, string.Empty),
            this);

    // ITracorData

    public object? this[string propertyName] {
        get {
            if (this.TryGetPropertyValue(propertyName, out var result)) {
                return result;
            } else { 
                return null;
            }
        }
    }

    public List<string> GetListPropertyName() {
        List<string> result = new(this.ListProperty.Count);
        foreach (var property in this.ListProperty) {
            result.Add(property.Name);
        }
        return result;
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        foreach (var property in this.ListProperty) {
            if (propertyName == property.Name) {
                propertyValue = property.Value;
                return true;
            }
        }
        propertyValue = null;
        return false;
    }

    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        listProperty.AddRange(this.ListProperty);
    }
}

public sealed class TracorDataRecordAccessorFactory : ITracorDataAccessorFactory<TracorDataRecord> {
    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is TracorDataRecord tracorDataValue) {
            tracorData = tracorDataValue; return true;
        }
        tracorData = default; return false;
    }

    public bool TryGetDataTyped(TracorDataRecord value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        tracorData = value;
        return true;
    }
}