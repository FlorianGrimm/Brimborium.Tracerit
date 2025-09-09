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
    /// Gets a dictionary of data accessor factories indexed by type.
    /// </summary>
    public Dictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByType { get; } = new ();

    /// <summary>
    /// Gets a list of general data accessor factories that can handle multiple types.
    /// </summary>
    public List<ITracorDataAccessorFactory> ListTracorDataAccessor { get; } = new ();

    /// <summary>
    /// Adds a strongly-typed data accessor factory for the specified type.
    /// </summary>
    /// <typeparam name="T">The type that the data accessor factory can handle.</typeparam>
    /// <param name="tracorDataAccessorFactory">The data accessor factory to add.</param>
    /// <returns>This <see cref="TracorValidatorOptions"/> instance for method chaining.</returns>
    public TracorValidatorOptions AddTracorDataAccessorByType<T>(ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        this.TracorDataAccessorByType.Add(typeof(T), tracorDataAccessorFactory);
        return this;
    }
}