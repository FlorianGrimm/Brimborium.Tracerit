namespace Brimborium.Tracerit;

/// <summary>
/// Configuration options for the Tracor validator, including data accessor factories for different types.
/// </summary>
public sealed class TracorDataConvertOptions {
    /// <summary>
    /// Initializes a new instance of the <see cref="TracorDataConvertOptions"/> class.
    /// </summary>
    public TracorDataConvertOptions() { }

    /// <summary>
    /// Allow dynamic converting using Reflection.
    /// </summary>
    public bool AllowReflection { get; set; }

    /// <summary>
    /// Gets a dictionary of data accessor factories indexed by TracorIdentifierType.
    /// </summary>
    public List<KeyValuePair<Type, ITracorDataAccessorFactory>> TracorDataAccessorByTypePrivate { get; } = new();

    /// <summary>
    /// Gets a dictionary of data accessor factories indexed by type.
    /// </summary>
    public Dictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByTypePublic { get; } = new();

    /// <summary>
    /// Gets a list of general data accessor factories that can handle multiple types.
    /// </summary>
    public List<ITracorDataAccessorFactory> ListTracorDataAccessor { get; } = new();

    public List<ITracorConvertObjectToListProperty> ListTracorConvertObjectToListProperty { get; } = new();
    
    public TracorDataConvertOptions AddTracorDataAccessorByTypePrivate<T>(ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        this.TracorDataAccessorByTypePrivate.Add(new (typeof(T), tracorDataAccessorFactory));
        return this;
    }
    public TracorDataConvertOptions AddTracorDataAccessorByTypePublic<T>(ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        this.TracorDataAccessorByTypePublic[typeof(T)] = tracorDataAccessorFactory;
        return this;
    }
}