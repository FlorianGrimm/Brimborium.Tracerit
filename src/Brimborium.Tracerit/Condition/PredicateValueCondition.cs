namespace Brimborium.Tracerit.Condition;

public sealed class PredicateTracorDataCondition : IExpressionCondition {
    private readonly Func<ITracorData, bool> _FnCondition;

    public PredicateTracorDataCondition(
        Func<ITracorData, bool> fnCondition
        ) {
        this._FnCondition = fnCondition;
    }
    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        return this._FnCondition(tracorData);
    }

    public static CalleeCondition operator *(TracorIdentitfier expected, PredicateTracorDataCondition and) {
        return new CalleeCondition(expected, and);
    }
}


public sealed class PredicateTracorDataCondition<TTracorData> : IExpressionCondition
    where TTracorData : ITracorData {
    private readonly Func<TTracorData, bool> _FnCondition;

    public PredicateTracorDataCondition(
        Func<TTracorData, bool> fnCondition
        ) {
        this._FnCondition = fnCondition;
    }
    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is TTracorData tracorDataTyped) {
            return this._FnCondition(tracorDataTyped);
        }
        return false;
    }

    public static CalleeCondition operator *(TracorIdentitfier expected, PredicateTracorDataCondition<TTracorData> and) {
        return new CalleeCondition(expected, and);
    }
}

public sealed class PredicateValueCondition<TValue> : IExpressionCondition<TValue> {
    private readonly Func<TValue, bool> _FnCondition;

    public PredicateValueCondition(
        Func<TValue, bool> fnCondition
        ) {
        this._FnCondition = fnCondition;
    }

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var value)) {
            return this._FnCondition(value);
        }
        return false;
    }

    public static CalleeCondition operator *(TracorIdentitfier expected, PredicateValueCondition<TValue> and) {
        return new CalleeCondition(expected, and);
    }
}

public sealed class PredicateValueGlobalStateCondition<TValue> : IExpressionCondition<TValue> {
    private readonly Func<TValue, TracorGlobalState, bool> _FnCondition;

    public PredicateValueGlobalStateCondition(
        Func<TValue, TracorGlobalState, bool> fnCondition
        ) {
        this._FnCondition = fnCondition;
    }

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var value)) {
            return this._FnCondition(value, currentContext.GlobalState);
        }
        return false;
    }

    public static CalleeCondition operator *(TracorIdentitfier expected, PredicateValueGlobalStateCondition<TValue> and) {
        return new CalleeCondition(expected, and);
    }
}

public sealed class PredicatePropertyCondition<TValue, TProperty> : IExpressionCondition {
    private readonly Func<TValue, TProperty> _FnGetProperty;
    private readonly TProperty _ExpectedValue;
    private readonly Func<TProperty, TProperty, bool> _FnEquality;
    private readonly string? _SetGlobalState;

    public PredicatePropertyCondition(
        Func<TValue, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default
        ) {
        this._FnGetProperty = fnGetProperty;
        this._ExpectedValue = expectedValue;
        this._FnEquality = fnEquality ?? EqualityComparer<TProperty>.Default.Equals;
        this._SetGlobalState = setGlobalState;
    }
    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var value)) {
            var propertyValue = this._FnGetProperty(value);
            var result = this._FnEquality(propertyValue, this._ExpectedValue);
            if (result) {
                if (!string.IsNullOrEmpty(this._SetGlobalState)) {
                    if (propertyValue is not null) {
                        currentContext.GlobalState[this._SetGlobalState] = propertyValue;
                    }
                }
            }
            return result;
        }
        return false;
    }

    public static CalleeCondition operator *(TracorIdentitfier expected, PredicatePropertyCondition<TValue, TProperty> and) {
        return new CalleeCondition(expected, and);
    }

}