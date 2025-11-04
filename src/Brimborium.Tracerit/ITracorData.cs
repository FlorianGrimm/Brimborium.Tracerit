namespace Brimborium.Tracerit;

/// <summary>
/// Represents trace data that can be inspected for properties and values.
/// </summary>
public interface ITracorData {
    /// <summary>
    /// Gets a list of all available property names in the trace data.
    /// </summary>
    /// <returns>A list of property names.</returns>
    List<string> GetListPropertyName();

    /// <summary>
    /// Attempts to get the value of a property by name.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="propertyValue">When this method returns, contains the property value if found; otherwise, null.</param>
    /// <returns>True if the property was found; otherwise, false.</returns>
    bool TryGetPropertyValue(string propertyName, out object? propertyValue);

    /// <summary>
    /// Try to get TracorDataProperty named <paramref name="propertyName"/>.
    /// </summary>
    /// <param name="propertyName">The property name to search fpr.</param>
    /// <param name="result">if return true - it contains the property</param>
    /// <returns>true if found.</returns>
    /// <remarks>it does not search in special properties</remarks>
    bool TryGetDataProperty(string propertyName, out TracorDataProperty result);

    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the identifier associated with this trace data record.
    /// </summary>
    TracorIdentifier TracorIdentifier { get; set; }

    /// <summary>
    /// Convert to <paramref name="listProperty"/>.
    /// </summary>
    void ConvertProperties(List<TracorDataProperty> listProperty);
}

/// <summary>
/// Represents trace data with a strongly-typed original value.
/// </summary>
/// <typeparam name="TValue">The type of the original value.</typeparam>
public interface ITracorData<TValue> : ITracorData {
    /// <summary>
    /// Attempts to get the original value that was used to create this trace data.
    /// </summary>
    /// <param name="value">When this method returns, contains the original value if available; otherwise, the default value.</param>
    /// <returns>True if the original value is available; otherwise, false.</returns>
    bool TryGetOriginalValue([MaybeNullWhen(false)] out TValue value);
}

/// <summary>
/// Factory interface for creating trace data from objects.
/// </summary>
public interface ITracorDataAccessorFactory {
    /// <summary>
    /// Attempts to create trace data from the specified value.
    /// </summary>
    /// <param name="value">The value to create trace data from.</param>
    /// <param name="tracorData">When this method returns, contains the trace data if successful; otherwise, null.</param>
    /// <returns>True if trace data was successfully created; otherwise, false.</returns>
    bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData);
}

/// <summary>
/// Generic factory interface for creating trace data from strongly-typed objects.
/// </summary>
/// <typeparam name="T">The type of objects this factory can process.</typeparam>
public interface ITracorDataAccessorFactory<T> : ITracorDataAccessorFactory {
    /// <summary>
    /// Attempts to create trace data from the specified strongly-typed value.
    /// </summary>
    /// <param name="value">The value to create trace data from.</param>
    /// <param name="tracorData">When this method returns, contains the trace data if successful; otherwise, null.</param>
    /// <returns>True if trace data was successfully created; otherwise, false.</returns>
    bool TryGetDataTyped(T value, [MaybeNullWhen(false)] out ITracorData tracorData);
}

/// <summary>
/// Provides access to properties of objects for tracing purposes.
/// </summary>
public interface ITracorDataAccessor {
    /// <summary>
    /// Gets a list of property names available on the specified value.
    /// </summary>
    /// <param name="value">The value to inspect.</param>
    /// <returns>A list of property names.</returns>
    List<string> GetListPropertyName(object value);

    /// <summary>
    /// Attempts to get a property value from the specified object.
    /// </summary>
    /// <param name="value">The object to get the property from.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="propertyValue">When this method returns, contains the property value if found; otherwise, null.</param>
    /// <returns>True if the property was found; otherwise, false.</returns>
    bool TryGetPropertyValueTyped(object value, string propertyName, out object? propertyValue);
}

/// <summary>
/// Provides strongly-typed access to properties of objects for tracing purposes.
/// </summary>
/// <typeparam name="T">The type of objects this accessor can process.</typeparam>
public interface ITracorDataAccessor<T> {
    /// <summary>
    /// Gets a list of property names available on the specified strongly-typed value.
    /// </summary>
    /// <param name="value">The value to inspect.</param>
    /// <returns>A list of property names.</returns>
    List<string> GetListPropertyNameTyped(T value);

    /// <summary>
    /// Attempts to get a property value from the specified strongly-typed object.
    /// </summary>
    /// <param name="value">The object to get the property from.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="propertyValue">When this method returns, contains the property value if found; otherwise, null.</param>
    /// <returns>True if the property was found; otherwise, false.</returns>
    bool TryGetPropertyValueTyped(T value, string propertyName, out object? propertyValue);

    /// <summary>
    /// Convert to <paramref name="listProperty"/>.
    /// </summary>
    void ConvertProperties(T value, List<TracorDataProperty> listProperty);
}

/// <summary>
/// Can convert itself for tracor
/// </summary>
public interface ITracorDataSelfAccessor {
    /// <summary>
    /// Convert Properties
    /// </summary>
    /// <param name="isPublic">true - public; false - private pip</param>
    /// <param name="tracorDataConvertService">convert service</param>
    /// <param name="listProperty">target</param>
    void ConvertProperties(bool isPublic, ITracorDataConvertService tracorDataConvertService, List<TracorDataProperty> listProperty);
}