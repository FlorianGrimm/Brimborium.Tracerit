namespace Brimborium.Tracerit.Service;

/// <summary>
/// Represents the global state for a tracor validation, storing key-value pairs that can be accessed and modified during validation.
/// </summary>
public struct TracorGlobalState {
    private readonly OnTraceStepExecutionState _ExecutionState;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorGlobalState"/> class.
    /// </summary>
    internal TracorGlobalState(OnTraceStepExecutionState executionState) {
        this._ExecutionState = executionState;
    }


    /// <summary>
    /// Sets a strongly-typed value for the specified key and returns this instance for method chaining.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>This <see cref="TracorGlobalState"/> instance for method chaining.</returns>
    public TracorGlobalState SetValue(TracorDataProperty value) {
        this._ExecutionState.DictGlobalState = this._ExecutionState.DictGlobalState.SetItem(value.Name, value);
        return this;
    }

    public bool TryGetValue(string name, out TracorDataProperty value) {
        if (this._ExecutionState.DictGlobalState.TryGetValue(name, out value)) {
            return true;
        } else {
            value = new TracorDataProperty(name);
            return false;
        }
    }

    public TracorDataProperty GetValue(string name) {
        if (this._ExecutionState.DictGlobalState.TryGetValue(name, out var value)) {
            return value;
        } else {
            return new TracorDataProperty(name);
        }
    }
}
