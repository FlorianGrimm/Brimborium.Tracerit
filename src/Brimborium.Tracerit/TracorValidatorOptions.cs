namespace Brimborium.Tracerit;

/// <summary>
/// Configuration options for the Tracor validator, including data accessor factories for different types.
/// </summary>
public sealed class TracorValidatorOptions {
    /// <summary>
    /// Initializes a new instance of the <see cref="TracorValidatorOptions"/> class.
    /// </summary>
    public TracorValidatorOptions() { }

    /// <summary>
    /// Gets a dictionary of data accessor factories indexed by TracorIdentitfierType.
    /// </summary>
    public List<KeyValuePair<TracorIdentitfierType, ITracorDataAccessorFactory>> TracorDataAccessorByTypePrivate { get; } = new();

    /// <summary>
    /// Gets a dictionary of data accessor factories indexed by type.
    /// </summary>
    public Dictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByTypePublic { get; } = new();

    /// <summary>
    /// Gets a list of general data accessor factories that can handle multiple types.
    /// </summary>
    public List<ITracorDataAccessorFactory> ListTracorDataAccessor { get; } = new();

    /// <summary>
    /// Adds a strongly-typed data accessor factory for the specified type.
    /// </summary>
    /// <typeparam name="T">The type that the data accessor factory can handle.</typeparam>
    /// <param name="tracorIdentitfier">the matching tracorIdentitfier</param>
    /// <param name="tracorDataAccessorFactory">the factory</param>
    /// <returns>fluent this</returns>
    public TracorValidatorOptions AddTracorDataAccessorByTypePrivate<T>(TracorIdentitfier tracorIdentitfier, ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        TracorIdentitfierType tracorIdentitfierType = new(tracorIdentitfier.Source, tracorIdentitfier.Scope, typeof(T));
        this.TracorDataAccessorByTypePrivate.Add(new (tracorIdentitfierType, tracorDataAccessorFactory));
        return this;
    }

    /// <summary>
    /// Adds a strongly-typed data accessor factory for the specified type.
    /// </summary>
    /// <typeparam name="T">The type that the data accessor factory can handle.</typeparam>
    /// <param name="tracorIdentitfierType"></param>
    /// <param name="tracorDataAccessorFactory"></param>
    /// <returns>fluent this</returns>
    /// <exception cref="ArgumentException">if <typeparamref name="T"/> does not matched <paramref name="tracorDataAccessorFactory"/> TypeParameter.</exception>
    public TracorValidatorOptions AddTracorDataAccessorByTypePrivate<T>(TracorIdentitfierType tracorIdentitfierType, ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        if (!typeof(T).Equals(tracorIdentitfierType.TypeParameter)) {
            throw new ArgumentException(nameof(tracorIdentitfierType));
        }
        this.TracorDataAccessorByTypePrivate.Add(new(tracorIdentitfierType, tracorDataAccessorFactory));
        return this;
    }

    /// <summary>
    /// Adds a strongly-typed data accessor factory for the specified type.
    /// </summary>
    /// <typeparam name="T">The type that the data accessor factory can handle.</typeparam>
    /// <param name="tracorDataAccessorFactory">The data accessor factory to add.</param>
    /// <returns>This <see cref="TracorValidatorOptions"/> instance for method chaining.</returns>
    public TracorValidatorOptions AddTracorDataAccessorByTypePublic<T>(ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        this.TracorDataAccessorByTypePublic[typeof(T)] = tracorDataAccessorFactory;
        return this;
    }
}