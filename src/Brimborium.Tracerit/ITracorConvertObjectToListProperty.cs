namespace Brimborium.Tracerit;

public interface ITracorConvertObjectToListProperty {
    Type GetValueType();

    void ConvertObjectToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        object? value,
        ITracorDataConvertService dataConvertService,
        List<TracorDataProperty> listProperty);
}

public interface ITracorConvertValueToListProperty<T> : ITracorConvertObjectToListProperty {
    void ConvertValueToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        T value,
        ITracorDataConvertService dataConvertService,
        List<TracorDataProperty> listProperty);
}
