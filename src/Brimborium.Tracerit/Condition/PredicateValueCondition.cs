using System.Runtime.CompilerServices;

namespace Brimborium.Tracerit.Condition;

public sealed class PredicateTracorDataCondition : IExpressionCondition {
    private readonly Func<ITracorData, bool> _FnCondition;
    private readonly string? _FnConditionDisplay;

    public PredicateTracorDataCondition(
        Func<ITracorData, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition = fnCondition;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        bool result = this._FnCondition(tracorData);
        currentContext.LoggerUtility.LogCondition(callee, result, this._FnConditionDisplay);
        return result;
    }

    public static OrCondition operator +(PredicateTracorDataCondition left, IExpressionCondition right) {
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateTracorDataCondition left, IExpressionCondition right) {
        return new AndCondition([left, right]);
    }
}


public sealed class PredicateTracorDataCondition<TTracorData> : IExpressionCondition
    where TTracorData : ITracorData {
    private readonly Func<TTracorData, bool> _FnCondition;
    private readonly string? _FnConditionDisplay;

    public PredicateTracorDataCondition(
        Func<TTracorData, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition = fnCondition;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }
    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is TTracorData tracorDataTyped) {
            var result = this._FnCondition(tracorDataTyped);
            currentContext.LoggerUtility.LogCondition(callee, result, this._FnConditionDisplay);
            return result;
        }
        return false;
    }

    public static OrCondition operator +(PredicateTracorDataCondition<TTracorData> left, IExpressionCondition right) {
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateTracorDataCondition<TTracorData> left, IExpressionCondition right) {
        return new AndCondition([left, right]);
    }
}

public sealed class PredicateValueCondition<TValue> : IExpressionCondition<TValue> {
    private readonly Func<TValue, bool> _FnCondition;
    private readonly string? _FnConditionDisplay;

    public PredicateValueCondition(
        Func<TValue, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition = fnCondition;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var value)) {
            bool result = this._FnCondition(value);
            currentContext.LoggerUtility.LogCondition(callee, result, this._FnConditionDisplay);
            return result;
        }
        return false;
    }

    public static OrCondition operator +(PredicateValueCondition<TValue> left, IExpressionCondition right) {
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateValueCondition<TValue> left, IExpressionCondition right) {
        return new AndCondition([left, right]);
    }
}

public sealed class PredicateValueGlobalStateCondition<TValue> : IExpressionCondition<TValue> {
    private readonly Func<TValue, TracorGlobalState, bool> _FnCondition;
    private readonly string? _FnConditionDisplay;

    public PredicateValueGlobalStateCondition(
        Func<TValue, TracorGlobalState, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition = fnCondition;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var value)) {
            bool result = this._FnCondition(value, currentContext.GlobalState);
            currentContext.LoggerUtility.LogCondition(callee, result, this._FnConditionDisplay);
            return result;
        }
        return false;
    }

    public static OrCondition operator +(PredicateValueGlobalStateCondition<TValue> left, IExpressionCondition right) {
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateValueGlobalStateCondition<TValue> left, IExpressionCondition right) {
        return new AndCondition([left, right]);
    }
}

