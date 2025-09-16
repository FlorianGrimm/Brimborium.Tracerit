
namespace Brimborium.Tracerit.DataAccessor;

public sealed class TracorDataAccessorFactory<TValue> : ITracorDataAccessorFactory<TValue> {
    private readonly ITracorDataAccessor<TValue> _TracorDataAccessor;

    public TracorDataAccessorFactory(
        ITracorDataAccessor<TValue> tracorDataAccessor) {
        this._TracorDataAccessor = tracorDataAccessor;
    }

    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is TValue valueT) {
            return this.TryGetDataTyped(valueT, out tracorData);
        }
        tracorData = default;
        return false;
    }

    public bool TryGetDataTyped(TValue value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        tracorData = new BoundAccessorTracorDataTyped<TValue>(this._TracorDataAccessor, value);
        return true;
    }
}
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
}
