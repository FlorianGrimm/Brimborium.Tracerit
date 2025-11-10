namespace Brimborium.Tracerit.Condition;

public sealed class PredicateValueCondition<TValue> : IExpressionCondition<TValue> {
    private readonly Func<TValue, bool>? _FnConditionBool;
    private readonly Func<TValue, TracorValidatorOnTraceResult>? _FnConditionOTR;
    private readonly string? _FnConditionDisplay;

    public PredicateValueCondition(
        Func<TValue, bool> fnConditionBool,
        [CallerArgumentExpression(nameof(fnConditionBool))] string? doNotPopulateThisValue = null
        ) {
        this._FnConditionBool = fnConditionBool;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public PredicateValueCondition(
       Func<TValue, TracorValidatorOnTraceResult> fnConditionOTR,
       [CallerArgumentExpression(nameof(fnConditionOTR))] string? doNotPopulateThisValue = null
       ) {
        this._FnConditionOTR = fnConditionOTR;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public TracorValidatorOnTraceResult DoesMatch(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        if (tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var value)) {
            if (this._FnConditionBool is { } fnConditionBool) {
                bool result = fnConditionBool(value);
                currentContext.LoggerUtility.LogConditionBool(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
                return result ? TracorValidatorOnTraceResult.Successful : TracorValidatorOnTraceResult.None;
            }
            if (this._FnConditionOTR is { } fnConditionOTR) {
                TracorValidatorOnTraceResult result = fnConditionOTR(value);
                currentContext.LoggerUtility.LogConditionOTR(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
                return result;
            }
        }
        return TracorValidatorOnTraceResult.None;
    }

    public static OrCondition operator +(PredicateValueCondition<TValue> left, IExpressionCondition right) {
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateValueCondition<TValue> left, IExpressionCondition right) {
        return new AndCondition([left, right]);
    }
}
