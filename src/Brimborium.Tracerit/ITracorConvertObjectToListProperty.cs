namespace Brimborium.Tracerit;

/// <summary>
/// Converts objects to a list of tracor data properties.
/// </summary>
public interface ITracorConvertObjectToListProperty {
    /// <summary>
    /// Gets the type of value this converter handles.
    /// </summary>
    /// <returns>The type that this converter can process.</returns>
    Type GetValueType();

    /// <summary>
    /// Converts an object to a list of trace data properties.
    /// </summary>
    /// <param name="isPublic">True if properties should be public; otherwise, false.</param>
    /// <param name="levelWatchDog">The recursion depth limit to prevent infinite loops.</param>
    /// <param name="name">The property name prefix.</param>
    /// <param name="value">The object to convert.</param>
    /// <param name="dataConvertService">The service used for converting nested objects.</param>
    /// <param name="listProperty">The target list to add properties to.</param>
    void ConvertObjectToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        object? value,
        ITracorDataConvertService dataConvertService,
        List<TracorDataProperty> listProperty);
}

/// <summary>
/// Converts strongly-typed values to a list of tracor data properties.
/// </summary>
/// <typeparam name="T">The type of values this converter handles.</typeparam>
public interface ITracorConvertValueToListProperty<T> : ITracorConvertObjectToListProperty {
    /// <summary>
    /// Converts a strongly-typed value to a list of trace data properties.
    /// </summary>
    /// <param name="isPublic">True if properties should be public; otherwise, false.</param>
    /// <param name="levelWatchDog">The recursion depth limit to prevent infinite loops.</param>
    /// <param name="name">The property name prefix.</param>
    /// <param name="value">The value to convert.</param>
    /// <param name="dataConvertService">The service used for converting nested objects.</param>
    /// <param name="listProperty">The target list to add properties to.</param>
    void ConvertValueToListProperty(
        bool isPublic,
        int levelWatchDog,
        string name,
        T value,
        ITracorDataConvertService dataConvertService,
        List<TracorDataProperty> listProperty);
}
