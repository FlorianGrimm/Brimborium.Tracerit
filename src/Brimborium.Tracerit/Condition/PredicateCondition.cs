namespace Brimborium.Tracerit.Condition;

public sealed class PredicateCondition : IExpressionCondition {
    private readonly Func<ITracorData, TracorForkState, TracorGlobalState, bool>? _FnConditionBool;
    private readonly Func<ITracorData, TracorForkState, TracorGlobalState, TracorValidatorOnTraceResult>? _FnConditionOTR;
    
    private readonly string? _FnConditionDisplay;

    public PredicateCondition(
        Func<ITracorData, TracorForkState, TracorGlobalState, bool> fnConditionBool,
        [CallerArgumentExpression(nameof(fnConditionBool))] string? doNotPopulateThisValue = null
        ) {
        this._FnConditionBool = fnConditionBool;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }


    public PredicateCondition(
        Func<ITracorData, TracorForkState, TracorGlobalState, TracorValidatorOnTraceResult> fnConditionOTR,
        [CallerArgumentExpression(nameof(fnConditionOTR))] string? doNotPopulateThisValue = null
        ) {
        this._FnConditionOTR = fnConditionOTR;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public TracorValidatorOnTraceResult DoesMatch(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        if (this._FnConditionBool is { } fnConditionBool) {
            bool result = fnConditionBool(tracorData, currentContext.ForkState, currentContext.GlobalState);
            currentContext.LoggerUtility.LogConditionBool(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
            return result ? TracorValidatorOnTraceResult.Successful : TracorValidatorOnTraceResult.None;
        }
        if (this._FnConditionOTR is { } fnConditionOTR) {
            TracorValidatorOnTraceResult result = fnConditionOTR(tracorData, currentContext.ForkState, currentContext.GlobalState);
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

