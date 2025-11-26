namespace Brimborium.Tracerit;

/// <summary>
/// Interface for objects that can convert themselves to a list of trace data properties.
/// </summary>
public interface ITracorConvertSelfToListProperty {
    /// <summary>
    /// Converts this object to a list of trace data properties.
    /// </summary>
    /// <param name="isPublic">True if properties should be public; otherwise, false.</param>
    /// <param name="levelWatchDog">The recursion depth limit to prevent infinite loops.</param>
    /// <param name="name">The property name prefix.</param>
    /// <param name="dataConvertService">The service used for converting nested objects.</param>
    /// <param name="listProperty">The target list to add properties to.</param>
    void ConvertSelfToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        ITracorDataConvertService dataConvertService,
        List<TracorDataProperty> listProperty);
}

/// <summary>
/// Extension methods for Tracor operations.
/// </summary>
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


    /// <summary>
    /// Gets the property prefix for complex/nested properties, or null if recursion limit is reached.
    /// </summary>
    /// <param name="levelWatchDog">The current recursion depth limit.</param>
    /// <param name="name">The base property name.</param>
    /// <returns>The property prefix with a dot separator, or null if recursion limit exceeded.</returns>
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