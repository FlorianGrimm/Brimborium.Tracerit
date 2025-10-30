namespace Brimborium.Tracerit.DataAccessor;

// TODO: remove after adjust tests
public static class ValueTracorData {
    public static ValueTracorData<TValue> Create<TValue>(TValue value) => new ValueTracorData<TValue>(value);
}

public sealed class ValueTracorData<TValue> : ITracorData<TValue> {
    private readonly TValue _Value;

    public ValueTracorData(TValue value) {
        this._Value = value;
    }

    public List<string> GetListPropertyName() {
        return [TracorConstants.TracorDataPropertyNameValue];
    }

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out TValue value) {
        value = this._Value;
        return true;
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        if (typeof(TValue).GetProperty(propertyName) is { } propertyInfo) {
            propertyValue = propertyInfo.GetValue(this._Value);
            return true;
        }

        if (TracorConstants.TracorDataPropertyNameValue == propertyName) {
            propertyValue = this._Value;
            return true;
        }
        propertyValue = null;
        return false;
    }

    /// <summary>
    /// Gets or sets the identifier associated with this trace data record.
    /// </summary>
    public TracorIdentifier TracorIdentifier { get; set; }

    public DateTime Timestamp { get; set; }

    public bool TryGetDataProperty(string propertyName, out TracorDataProperty result) {
        if (typeof(TValue).GetProperty(propertyName) is { } propertyInfo) {
            var propertyValue = propertyInfo.GetValue(this._Value);
            result = TracorDataProperty.Create(propertyName, propertyValue);
            return true;
        }

        if (TracorConstants.TracorDataPropertyNameValue == propertyName) {
            result = TracorDataProperty.Create(propertyName, this._Value);
            return true;
        }

        result = new TracorDataProperty(string.Empty);
        return false;
    }

    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        listProperty.Add(
            TracorDataProperty.Create(
                TracorConstants.TracorDataPropertyNameValue,
                this._Value));
    }
}
