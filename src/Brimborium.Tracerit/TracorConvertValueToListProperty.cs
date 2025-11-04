namespace Brimborium.Tracerit;

public abstract class TracorConvertValueToListProperty<T> : ITracorConvertValueToListProperty<T> {

    protected TracorConvertValueToListProperty() {
    }

    public Type GetValueType() => typeof(T);

    public void ConvertObjectToListProperty(bool isPublic, int levelWatchDog, string name, object? value, List<TracorDataProperty> listProperty) {
        if (value is null) { return; }
        if (value is T valueT) {
            this.ConvertValueToListProperty(isPublic, levelWatchDog, name, valueT, listProperty);
        }
    }

    public abstract void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, T value, List<TracorDataProperty> listProperty);

}
public abstract class TracorConvertValueToListPropertyWithService<T> : ITracorConvertValueToListProperty<T> {
    private readonly LateTracorDataConvertService _LateTracorDataConvertService;

    protected TracorConvertValueToListPropertyWithService(
        LateTracorDataConvertService lateTracorDataConvertService) {
        this._LateTracorDataConvertService = lateTracorDataConvertService;
    }

    public Type GetValueType() => typeof(T);

    protected ITracorDataConvertService GetTracorDataConvertService()
        => this._LateTracorDataConvertService.GetTracorDataConvertService();

    public void ConvertObjectToListProperty(bool isPublic, int levelWatchDog, string name, object? value, List<TracorDataProperty> listProperty) {
        if (value is null) { return; }
        if (value is T valueT) {
            this.ConvertValueToListProperty(isPublic, levelWatchDog, name, valueT, listProperty);
        }
    }

    public abstract void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, T value, List<TracorDataProperty> listProperty);

}


