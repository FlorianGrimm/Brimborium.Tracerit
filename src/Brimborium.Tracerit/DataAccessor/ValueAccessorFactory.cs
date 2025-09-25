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
