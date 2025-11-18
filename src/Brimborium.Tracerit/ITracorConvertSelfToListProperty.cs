namespace Brimborium.Tracerit;

public interface ITracorConvertSelfToListProperty {
    void ConvertSelfToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        ITracorDataConvertService dataConvertService,
        List<TracorDataProperty> listProperty);
}

public static partial class TracorExtension {
    //public static (bool success, string prefix) GetComplexPropertyPrefix(
    //    int levelWatchDog,
    //    string name) {
    //    if (levelWatchDog < 0) {
    //        return (success: false, prefix: "#TooDeep#");
    //    } else {
    //        return (success: true, prefix: name is { Length: 0 } ? name : $"{name}.");
    //    }
    //}

    public static string? GetComplexPropertyPrefix(
        int levelWatchDog,
        string name) {
        if (levelWatchDog < 0) {
            return null;
        } else {
            return name is { Length: 0 } ? name : $"{name}.";
        }
    }
}