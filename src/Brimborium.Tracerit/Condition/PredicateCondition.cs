namespace Brimborium.Tracerit.Condition;

public sealed class PredicateCondition : IExpressionCondition {
    private readonly Func<ITracorData, bool>? _FnCondition1Bool;
    private readonly Func<ITracorData, TracorGlobalState, bool>? _FnCondition2Bool;
    private readonly Func<ITracorData, TracorValidatorOnTraceResult>? _FnCondition1OTR;
    private readonly Func<ITracorData, TracorGlobalState, TracorValidatorOnTraceResult>? _FnCondition2OTR;
    private readonly string? _FnConditionDisplay;

    public PredicateCondition(
        Func<ITracorData, bool> fnCondition1Bool,
        [CallerArgumentExpression(nameof(fnCondition1Bool))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition1Bool = fnCondition1Bool;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public PredicateCondition(
        Func<ITracorData, TracorValidatorOnTraceResult> fnCondition1OTR,
        [CallerArgumentExpression(nameof(fnCondition1OTR))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition1OTR = fnCondition1OTR;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public PredicateCondition(
        Func<ITracorData, TracorGlobalState, bool> fnConditionBool,
        [CallerArgumentExpression(nameof(fnConditionBool))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition2Bool = fnConditionBool;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public PredicateCondition(
        Func<ITracorData, TracorGlobalState, TracorValidatorOnTraceResult> fnConditionOTR,
        [CallerArgumentExpression(nameof(fnConditionOTR))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition2OTR = fnConditionOTR;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public TracorValidatorOnTraceResult DoesMatch(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        if (this._FnCondition1Bool is { } fnCondition1Bool) {
            bool result = fnCondition1Bool(tracorData);
            currentContext.LoggerUtility.LogConditionBool(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
            return result ? TracorValidatorOnTraceResult.Successful : TracorValidatorOnTraceResult.None;
        }
        if (this._FnCondition2Bool is { } fnCondition2Bool) {
            bool result = fnCondition2Bool(tracorData, currentContext.GlobalState);
            currentContext.LoggerUtility.LogConditionBool(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
            return result ? TracorValidatorOnTraceResult.Successful : TracorValidatorOnTraceResult.None;
        }
        if (this._FnCondition1OTR is { } fnCondition1OTR) {
            TracorValidatorOnTraceResult result = fnCondition1OTR(tracorData);
            currentContext.LoggerUtility.LogConditionOTR(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
            return result;
        }
        if (this._FnCondition2OTR is { } fnCondition2OTR) {
            TracorValidatorOnTraceResult result = fnCondition2OTR(tracorData, currentContext.GlobalState);
            currentContext.LoggerUtility.LogConditionOTR(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
            return result;
        }
        return TracorValidatorOnTraceResult.Failed;
    }

    public static OrCondition operator +(PredicateCondition left, IExpressionCondition right) {
        if (right is OrCondition orConditionRight) {
            return new OrCondition([left, .. orConditionRight.ExpressionConditions]);
        }
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateCondition left, IExpressionCondition right) {
        if (right is AndCondition orConditionRight) {
            return new AndCondition([left, .. orConditionRight.ExpressionConditions]);
        }
        return new AndCondition([left, right]);
    }
}

