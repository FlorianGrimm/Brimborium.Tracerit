namespace Brimborium.Tracerit.Service;

/// <summary>
/// Represents the global state for a tracor validation, storing key-value pairs that can be accessed and modified during validation.
/// </summary>
public sealed class TracorGlobalState : Dictionary<string, TracorDataProperty> {
    public readonly Lock Lock = new Lock();

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorGlobalState"/> class.
    /// </summary>
    public TracorGlobalState() {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorGlobalState"/> class with values copied from the specified dictionary.
    /// </summary>
    /// <param name="src">The source dictionary to copy values from.</param>
    public TracorGlobalState(Dictionary<string, TracorDataProperty> src) : base(src) {
    }

    /// <summary>
    /// Sets a strongly-typed value for the specified key and returns this instance for method chaining.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>This <see cref="TracorGlobalState"/> instance for method chaining.</returns>
    public TracorGlobalState SetValue(TracorDataProperty value) {
        this[value.Name] = value;
        return this;
    }

    public TracorDataProperty GetValue(string name) {
        if (this.TryGetValue(name, out var value)) {
            return value;
        } else {
            return new TracorDataProperty(name);
        }
    }
}
