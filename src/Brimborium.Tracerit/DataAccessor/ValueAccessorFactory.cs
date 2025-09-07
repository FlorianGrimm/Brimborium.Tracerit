namespace Brimborium.Tracerit.DataAccessor;

public sealed class ValueAccessorFactory<TValue>
    : ITracorDataAccessorFactory<TValue> {
    public ValueAccessorFactory() {
    }

    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is TValue valueTyped) {
            return this.TryGetDataTyped(valueTyped, out tracorData);
        }
        tracorData = default;
        return false;
    }

    public bool TryGetDataTyped(TValue value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        tracorData = new ValueTracorData<TValue>(value);
        return true;
    }
}
public static class ValueTracorData {
    public static ValueTracorData<TValue> Create<TValue>(TValue value) => new ValueTracorData<TValue>(value);
}
public sealed class ValueTracorData<TValue>
    : ITracorData<TValue> {
    private readonly TValue _Value;

    public ValueTracorData(TValue value) {
        this._Value = value;
    }

    public List<string> GetListPropertyName() => ["Value"];

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out TValue value) {
        value = this._Value;
        return true;
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        if ("Value" == propertyName) {
            propertyValue = this._Value;
            return true;
        }
        propertyValue = null;
        return false;
    }
}
