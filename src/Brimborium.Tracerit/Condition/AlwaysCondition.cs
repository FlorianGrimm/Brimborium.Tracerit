namespace Brimborium.Tracerit.Condition;

public sealed class AlwaysCondition : IExpressionCondition {
    private static AlwaysCondition? _Instance;
    public static IExpressionCondition Instance => _Instance ??= new AlwaysCondition();


    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) => true;
}

public sealed class AlwaysCondition<TValue, TProperty> : IExpressionCondition {
    private readonly Func<TValue, TProperty> _FnGetProperty;
    private readonly string _SetGlobalState;

    public AlwaysCondition(
        Func<TValue, TProperty> fnGetProperty,
        string setGlobalState
        ) {
        this._FnGetProperty = fnGetProperty;
        this._SetGlobalState = setGlobalState;
    }

    public bool DoesMatch(
        TracorIdentitfier callee,
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