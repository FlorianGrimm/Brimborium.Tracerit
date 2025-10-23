namespace Brimborium.Tracerit.DataAccessor;

public sealed class BoundAccessorTracorDataTyped<TValue> : ITracorData<TValue> {
    private readonly ITracorDataAccessor<TValue> _TracorDataAccessor;
    private readonly TValue _Value;

    public BoundAccessorTracorDataTyped(ITracorDataAccessor<TValue> tracorDataAccessor, TValue value) {
        this._TracorDataAccessor = tracorDataAccessor;
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

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out TValue value) {
        value = this._Value;
        return true;
    }

    public List<string> GetListPropertyName() {
        return this._TracorDataAccessor.GetListPropertyNameTyped(this._Value);
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        return this._TracorDataAccessor.TryGetPropertyValueTyped(this._Value, propertyName, out propertyValue);
    }

    /// <summary>
    /// Gets or sets the identifier associated with this trace data record.
    /// </summary>
    public TracorIdentifier TracorIdentifier { get; set; }

    public DateTime Timestamp { get; set; }

    public bool TryGetDataProperty(string propertyName, out TracorDataProperty result) {
        if (this._TracorDataAccessor.TryGetPropertyValueTyped(this._Value, propertyName, out var propertyValue)) {
            result = TracorDataProperty.Create(propertyName, propertyValue);
            return true;
        }

        result = new TracorDataProperty(string.Empty);
        return false;
    }

    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        var tracorDataPropertyValue = TracorDataProperty.Create(
            TracorConstants.TracorDataPropertyNameValue,
            this._Value);
        listProperty.Add(tracorDataPropertyValue);
    }
}
