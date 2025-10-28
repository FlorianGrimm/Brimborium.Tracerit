namespace Brimborium.Tracerit.Condition;

public sealed class PredicateTracorDataCondition : IExpressionCondition {
    private readonly Func<ITracorData, bool>? _FnConditionBool;
    private readonly Func<ITracorData, TracorValidatorOnTraceResult>? _FnConditionOTR;
    private readonly string? _FnConditionDisplay;

    public PredicateTracorDataCondition(
        Func<ITracorData, bool> fnConditionBool,
        [CallerArgumentExpression(nameof(fnConditionBool))] string? doNotPopulateThisValue = null
        ) {
        this._FnConditionBool = fnConditionBool;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public PredicateTracorDataCondition(
        Func<ITracorData, TracorValidatorOnTraceResult> fnConditionOTR,
        [CallerArgumentExpression(nameof(fnConditionOTR))] string? doNotPopulateThisValue = null
        ) {
        this._FnConditionOTR = fnConditionOTR;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }


    public TracorValidatorOnTraceResult DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (this._FnConditionBool is { } fnConditionBool) {
            bool result = fnConditionBool(tracorData);
            currentContext.LoggerUtility.LogConditionBool(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
            return result ? TracorValidatorOnTraceResult.Successful : TracorValidatorOnTraceResult.None;
        }
        if (this._FnConditionOTR is { } fnConditionOTR) {
            TracorValidatorOnTraceResult result = fnConditionOTR(tracorData);
            currentContext.LoggerUtility.LogConditionOTR(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
            return result;
        }
        return TracorValidatorOnTraceResult.Failed;
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
    private readonly Func<TTracorData, bool>? _FnConditionBool;
    private readonly Func<TTracorData, TracorValidatorOnTraceResult>? _FnConditionOTR;
    private readonly string? _FnConditionDisplay;

    public PredicateTracorDataCondition(
        Func<TTracorData, bool> fnConditionBool,
        [CallerArgumentExpression(nameof(fnConditionBool))] string? doNotPopulateThisValue = null
        ) {
        this._FnConditionBool = fnConditionBool;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public PredicateTracorDataCondition(
        Func<TTracorData, TracorValidatorOnTraceResult> fnConditionOTR,
        [CallerArgumentExpression(nameof(fnConditionOTR))] string? doNotPopulateThisValue = null
        ) {
        this._FnConditionOTR = fnConditionOTR;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public TracorValidatorOnTraceResult DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (this._FnConditionBool is { } fnConditionBool) {
            if (tracorData is TTracorData tracorDataTyped) {
                var result = fnConditionBool(tracorDataTyped);
                currentContext.LoggerUtility.LogConditionBool(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
                return result ? TracorValidatorOnTraceResult.Successful : TracorValidatorOnTraceResult.None;
            } else {
                return TracorValidatorOnTraceResult.None;
            }
        }
        if (this._FnConditionOTR is { } fnConditionOTR) {
            if (tracorData is TTracorData tracorDataTyped) {
                var result = fnConditionOTR(tracorDataTyped);
                currentContext.LoggerUtility.LogConditionOTR(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
                return result;
            } else {
                return TracorValidatorOnTraceResult.None;
            }
        }
        return TracorValidatorOnTraceResult.Failed;
    }

    public static OrCondition operator +(PredicateTracorDataCondition<TTracorData> left, IExpressionCondition right) {
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateTracorDataCondition<TTracorData> left, IExpressionCondition right) {
        return new AndCondition([left, right]);
    }
}