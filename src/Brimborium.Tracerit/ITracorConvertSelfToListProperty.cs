namespace Brimborium.Tracerit;

public interface ITracorConvertSelfToListProperty {
    void ConvertSelfToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        ITracorDataConvertService dataConvertService,
        List<TracorDataProperty> listProperty);
}
