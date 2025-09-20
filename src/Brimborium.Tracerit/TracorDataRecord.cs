
namespace Brimborium.Tracerit;
public sealed class TracorDataRecord : ITracorData {
    private TracorDataRecordOperation _Operation;
    private TracorIdentitfier? _TracorIdentitfier;

    public TracorDataRecord() { }

    public TracorIdentitfier? TracorIdentitfier {
        get {
            return this._TracorIdentitfier;
        }

        set {
            if (value is { } ti) {
                this._Operation = ti.GetOperation();
            }
            this._TracorIdentitfier = value;
        }
    }

    public List<TracorDataProperty> ListProperty { get; } = [];

    public TracorIdentitfierData ToTracorIdentitfierData()
        => new TracorIdentitfierData(
            this._TracorIdentitfier ?? new(string.Empty, string.Empty),
            this);

    // think

    public TracorDataRecordOperation GetOperation() {
        if (TracorDataRecordOperation.Unknown == this._Operation) {
            if (this._TracorIdentitfier is { } ti) {
                this._Operation = ti.GetOperation();
            } else {
                this._Operation = TracorDataRecordOperation.Data;
            }
        }
        return this._Operation;
    }

    public void SetOperation(TracorDataRecordOperation value) {
        this._Operation = value;
        this._TracorIdentitfier = TracorIdentitfier.CreateForOperation(value) ?? this._TracorIdentitfier;
    }

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

    public static bool IsPartialEquals(TracorIdentitfierData currentData, TracorDataRecord expectedData) {
        if (expectedData.TracorIdentitfier is { } expectedtracorIdentitfier) {
            var currentTracorIdentitfier = currentData.TracorIdentitfier;
            if (!MatchEqualityComparerTracorIdentitfier.Default.Equals(
                    currentTracorIdentitfier,
                    expectedtracorIdentitfier)) {
                return false;
            }
        }
        if (0 < expectedData.ListProperty.Count) {
            foreach (var expectedProperty in expectedData.ListProperty) {
                if (currentData.TracorData.TryGetPropertyValue(expectedProperty.Name, out var currentPropertyValue)) {
                    if (expectedProperty.HasEqualValue(currentPropertyValue)) {
                        // equal -> ok
                    } else {
                        // not equal
                        return false;
                    }
                } else {
                    // not found
                    return false;
                }
            }
        }

        // no diff found
        return true;
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