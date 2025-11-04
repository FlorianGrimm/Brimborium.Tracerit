namespace Brimborium.Tracerit;

public interface ITracorConvertSelfToListProperty {
    void ConvertSelfToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        List<TracorDataProperty> listProperty);
}
