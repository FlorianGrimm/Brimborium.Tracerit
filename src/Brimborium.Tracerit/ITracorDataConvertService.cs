namespace Brimborium.Tracerit;

public interface ITracorDataConvertService {
    ITracorData ConvertPrivate<T>(TracorIdentifier callee, T value);
    ITracorData ConvertPublic<T>(T value);

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

public interface ITracorConvertObjectToListProperty {
    Type GetValueType();

    void ConvertObjectToListProperty(
            bool isPublic,
            int levelWatchDog,
            string name,
            object? value,
            List<TracorDataProperty> listProperty);
}
public interface ITracorConvertValueToListProperty<T> : ITracorConvertObjectToListProperty {
    void ConvertValueToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        T value,
        List<TracorDataProperty> listProperty);
}

public interface ITracorConvertSelfToListProperty {
    void ConvertSelfToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        List<TracorDataProperty> listProperty);
}

public abstract class TracorConvertValueToListProperty<T> : ITracorConvertValueToListProperty<T> {
    public Type GetValueType() => typeof(T);

    public void ConvertObjectToListProperty(bool isPublic, int levelWatchDog, string name, object? value, List<TracorDataProperty> listProperty) {
        if (value is null) { return; }
        if (value is T valueT) {
            this.ConvertValueToListProperty(isPublic, levelWatchDog, name, valueT, listProperty);
        }
    }

    public abstract void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, T value, List<TracorDataProperty> listProperty);

}