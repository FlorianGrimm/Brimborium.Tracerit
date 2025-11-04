namespace Brimborium.Tracerit;

public interface ITracorDataConvertService {
    ITracorData ConvertPrivate<T>(TracorIdentifier callee, T value);

    ITracorData ConvertPublic<T>(TracorIdentifier callee, T value);

    ITracorConvertObjectToListProperty? GetTracorConvertObjectToListProperty(Type typeValue);

    ITracorConvertValueToListProperty<T>? GetConverterValueListProperty<T>();

    void ConvertObjectToListProperty(
            bool isPublic,
            int levelWatchDog,
            string name,
            object? value,
            List<TracorDataProperty> listProperty);

    void ConvertValueToListProperty<T>(
        bool isPublic,
        int levelWatchDog,
        string name,
        T value, 
        List<TracorDataProperty> listProperty);
}
