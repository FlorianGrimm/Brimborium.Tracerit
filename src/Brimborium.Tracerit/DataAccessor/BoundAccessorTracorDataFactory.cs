namespace Brimborium.Tracerit.DataAccessor;

public sealed class BoundAccessorTracorDataFactory<TValue> : ITracorDataAccessorFactory<TValue> {
    private readonly ITracorDataAccessor<TValue> _TracorDataAccessor;
    private readonly TracorDataRecordPool _TracorDataRecordPool;

    public BoundAccessorTracorDataFactory(
        ITracorDataAccessor<TValue> tracorDataAccessor, 
        TracorDataRecordPool tracorDataRecordPool) {
        this._TracorDataAccessor = tracorDataAccessor;
        this._TracorDataRecordPool = tracorDataRecordPool;
    }

    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is TValue valueT) {
            return this.TryGetDataTyped(valueT, out tracorData);
        }
        tracorData = default;
        return false;
    }

    public bool TryGetDataTyped(TValue value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        var tracorDataRecord = this._TracorDataRecordPool.Rent();
        TracorDataUtility.SetActivity(tracorDataRecord.ListProperty);
        this._TracorDataAccessor.ConvertProperties(value, tracorDataRecord.ListProperty);
        tracorData = tracorDataRecord;
        return true;
    }
}
