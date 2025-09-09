namespace Brimborium.Tracerit.Service;

/// <summary>
/// Represents the global state for a tracor validation, storing key-value pairs that can be accessed and modified during validation.
/// This class extends <see cref="Dictionary{TKey, TValue}"/> to provide strongly-typed access methods.
/// </summary>
public sealed class TracorGlobalState : Dictionary<string, object> {
    /// <summary>
    /// Initializes a new instance of the <see cref="TracorGlobalState"/> class.
    /// </summary>
    public TracorGlobalState() {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorGlobalState"/> class with values copied from the specified dictionary.
    /// </summary>
    /// <param name="src">The source dictionary to copy values from.</param>
    public TracorGlobalState(Dictionary<string, object> src) : base(src) {
    }

    /// <summary>
    /// Sets a strongly-typed value for the specified key and returns this instance for method chaining.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>This <see cref="TracorGlobalState"/> instance for method chaining.</returns>
    public TracorGlobalState SetValue<T>(string key, T value) where T:notnull {
        this[key] = value;
        return this;
    }

    /// <summary>
    /// Gets a strongly-typed value for the specified key.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value cast to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be cast to the specified type.</exception>
    public T GetValue<T>(string key) => (T)this[key];
}
