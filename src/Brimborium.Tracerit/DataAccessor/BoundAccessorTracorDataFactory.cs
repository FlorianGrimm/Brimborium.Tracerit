namespace Brimborium.Tracerit.DataAccessor;

public sealed class BoundAccessorTracorDataFactory<TValue> : ITracorDataAccessorFactory<TValue> {
    private readonly ITracorDataAccessor<TValue> _TracorDataAccessor;

    public BoundAccessorTracorDataFactory(
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
