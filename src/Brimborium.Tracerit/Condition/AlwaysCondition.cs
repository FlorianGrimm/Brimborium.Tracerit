namespace Brimborium.Tracerit.Condition;

/// <summary>
/// Represents a condition that always evaluates to true, regardless of the trace data.
/// This is a singleton implementation for performance optimization.
/// </summary>
public sealed class AlwaysCondition : IExpressionCondition {
    private static AlwaysCondition? _Instance;

    /// <summary>
    /// Gets the singleton instance of the <see cref="AlwaysCondition"/>.
    /// </summary>
    public static IExpressionCondition Instance => _Instance ??= new AlwaysCondition();

    /// <inheritdoc />
    /// <summary>
    /// Always returns true, indicating that this condition is always satisfied.
    /// </summary>
    /// <param name="tracorData">The trace data to evaluate.</param>
    /// <param name="currentContext">The current context of the validation step.</param>
    /// <returns>Always returns true.</returns>
    public bool DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) => true;
}

/// <summary>
/// Represents a strongly-typed condition that always evaluates to true and can extract a property value to set in the global state.
/// </summary>
/// <typeparam name="TValue">The type of the value to extract the property from.</typeparam>
/// <typeparam name="TProperty">The type of the property to extract.</typeparam>
public sealed class AlwaysCondition<TValue, TProperty> : IExpressionCondition {
    private readonly Func<TValue, TProperty> _FnGetProperty;
    private readonly string _SetGlobalState;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlwaysCondition{TValue, TProperty}"/> class.
    /// </summary>
    /// <param name="fnGetProperty">A function to extract the property value from the trace data.</param>
    /// <param name="setGlobalState">The key to use when setting the extracted property value in the global state.</param>
    public AlwaysCondition(
        Func<TValue, TProperty> fnGetProperty,
        string setGlobalState
        ) {
        this._FnGetProperty = fnGetProperty;
        this._SetGlobalState = setGlobalState;
    }

    public bool DoesMatch(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        if (this._FnGetProperty is { } fnGetProperty
            && this._SetGlobalState is { Length: > 0 } setGlobalState
            && tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var valueTyped)) {
            if (fnGetProperty(valueTyped) is { } propertyValue) {
                currentContext.GlobalState[setGlobalState] = propertyValue;
            }
        }
        return true;
    }
}