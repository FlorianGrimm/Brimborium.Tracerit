namespace Brimborium.Tracerit.DataAccessor;

public static class ValueTracorData {
    public static ValueTracorData<TValue> Create<TValue>(TValue value) => new ValueTracorData<TValue>(value);
}

public sealed class ValueTracorData<TValue> : ITracorData<TValue> {
    private readonly TValue _Value;

    public ValueTracorData(TValue value) {
        this._Value = value;
    }

    public object? this[string propertyName] {
        get {
            if (this.TryGetPropertyValue(propertyName, out var propertyValue)) {
                return propertyValue;
            } else {
                return null;
            }
        }
    }

    public List<string> GetListPropertyName() => ["Value"];

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out TValue value) {
        value = this._Value;
        return true;
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        if (typeof(TValue).GetProperty(propertyName) is { } propertyInfo) {
            propertyValue=propertyInfo.GetValue(this._Value);
            return true;
        }

        if ("Value" == propertyName) {
            propertyValue = this._Value;
            return true;
        }
        propertyValue = null;
        return false;
    }

    /// <summary>
    /// Gets or sets the identifier associated with this trace data record.
    /// </summary>
    public TracorIdentitfier TracorIdentitfier { get; set; }

    public DateTime Timestamp { get; set; }

    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        // TODO: if needed
    }
}
