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
    /// Gets a dictionary of data accessor factories indexed by TracorIdentifierType.
    /// </summary>
    public List<KeyValuePair<TracorIdentifierType, ITracorDataAccessorFactory>> TracorDataAccessorByTypePrivate { get; } = new();

    /// <summary>
    /// Gets a dictionary of data accessor factories indexed by type.
    /// </summary>
    public Dictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByTypePublic { get; } = new();

    /// <summary>
    /// Gets a list of general data accessor factories that can handle multiple types.
    /// </summary>
    public List<ITracorDataAccessorFactory> ListTracorDataAccessor { get; } = new();

    public List<ITracorConvertObjectToListProperty> ListTracorConvertToListProperty { get; } = new();
    
    /// <summary>
    /// Adds a strongly-typed data accessor factory for the specified type.
    /// </summary>
    /// <typeparam name="T">The type that the data accessor factory can handle.</typeparam>
    /// <param name="tracorIdentifier">the matching tracorIdentifier</param>
    /// <param name="tracorDataAccessorFactory">the factory</param>
    /// <returns>fluent this</returns>
    public TracorDataConvertOptions AddTracorDataAccessorByTypePrivate<T>(TracorIdentifier tracorIdentifier, ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        TracorIdentifierType tracorIdentifierType = new(tracorIdentifier.Source, tracorIdentifier.Scope, typeof(T));
        this.TracorDataAccessorByTypePrivate.Add(new (tracorIdentifierType, tracorDataAccessorFactory));
        return this;
    }

    /// <summary>
    /// Adds a strongly-typed data accessor factory for the specified type.
    /// </summary>
    /// <typeparam name="T">The type that the data accessor factory can handle.</typeparam>
    /// <param name="tracorIdentifierType"></param>
    /// <param name="tracorDataAccessorFactory"></param>
    /// <returns>fluent this</returns>
    /// <exception cref="ArgumentException">if <typeparamref name="T"/> does not matched <paramref name="tracorDataAccessorFactory"/> TypeParameter.</exception>
    public TracorDataConvertOptions AddTracorDataAccessorByTypePrivate<T>(TracorIdentifierType tracorIdentifierType, ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        if (!typeof(T).Equals(tracorIdentifierType.TypeParameter)) {
            throw new ArgumentException("Mismatch T and TypeParameter", nameof(tracorIdentifierType));
        }
        this.TracorDataAccessorByTypePrivate.Add(new(tracorIdentifierType, tracorDataAccessorFactory));
        return this;
    }

    /// <summary>
    /// Adds a strongly-typed data accessor factory for the specified type.
    /// </summary>
    /// <typeparam name="T">The type that the data accessor factory can handle.</typeparam>
    /// <param name="tracorDataAccessorFactory">The data accessor factory to add.</param>
    /// <returns>This <see cref="TracorDataConvertOptions"/> instance for method chaining.</returns>
    public TracorDataConvertOptions AddTracorDataAccessorByTypePublic<T>(ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        this.TracorDataAccessorByTypePublic[typeof(T)] = tracorDataAccessorFactory;
        return this;
    }
}