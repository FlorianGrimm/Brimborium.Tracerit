namespace Brimborium.Tracerit.Service;

internal sealed class TracorConvertSelfToListPropertyAdapter<T>
    : ITracorConvertValueToListProperty<T>
    where T : ITracorConvertSelfToListProperty {
    public TracorConvertSelfToListPropertyAdapter() { }

    public Type GetValueType() => typeof(T);

    public void ConvertObjectToListProperty(bool isPublic, int levelWatchDog, string name, object? value, List<TracorDataProperty> listProperty) {
        if (value is null) { return; }
        if (value is T valueT) {
            valueT.ConvertSelfToListProperty(isPublic, levelWatchDog, name, listProperty);
        }
    }

    public void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, T value, List<TracorDataProperty> listProperty) {
        value.ConvertSelfToListProperty(isPublic, levelWatchDog, name, listProperty);
    }
}