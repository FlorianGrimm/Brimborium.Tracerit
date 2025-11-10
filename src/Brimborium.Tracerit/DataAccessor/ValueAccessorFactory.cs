namespace Brimborium.Tracerit.DataAccessor;

public sealed class ValueAccessorFactory<TValue>
    : ITracorDataAccessorFactory<TValue> {
    private readonly TracorDataRecordPool _TracorDataRecordPool;

    public ValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) {
        this._TracorDataRecordPool = tracorDataRecordPool;
    }

    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is TValue valueTyped) {
            return this.TryGetDataTyped(valueTyped, out tracorData);
        }
        tracorData = default;
        return false;
    }

    public bool TryGetDataTyped(TValue value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        var tracorDataRecord = this._TracorDataRecordPool.Rent();
        TracorDataUtility.SetActivity(tracorDataRecord.ListProperty);
        ValueAccessorFactoryUtility.Convert(value!, tracorDataRecord.ListProperty);
        tracorData = tracorDataRecord;
        return true;
    }
}

public static class ValueAccessorFactoryUtility {
    public static void Convert<TValue>(
        [DisallowNull] TValue value, 
        List<TracorDataProperty> listProperty
        /* TODO: ITracorDataConvertService? tracorDataConvertService*/
        ) {
        var tdp=TracorDataProperty.Create("value", value);
        if (tdp.TypeValue != TracorDataPropertyTypeValue.Any) {
            listProperty.Add(tdp);
        } else {
            Type type = typeof(TValue);
            foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                if (propertyInfo.CanRead) {
                    var propertyValue = propertyInfo.GetValue(value);
                    if (propertyValue is null) { continue; }
                    //if (tracorDataConvertService is { }) {
                    //    tracorDataConvertService.ConvertPublic(propertyValue)
                    //}
                    listProperty.Add(TracorDataProperty.Create(propertyInfo.Name, propertyValue));
                }
            }
        }
    }
}
