namespace Brimborium.Tracerit.Condition;

public sealed class PredicateCondition : IExpressionCondition {
    private readonly Func<ITracorData, TracorForkState, TracorGlobalState, bool> _FnCondition;
    private readonly string? _FnConditionDisplay;

    public PredicateCondition(
        Func<ITracorData, TracorForkState, TracorGlobalState, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition = fnCondition;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public bool DoesMatch(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        
        bool result = this._FnCondition(tracorData, currentContext.ForkState, currentContext.GlobalState);
        currentContext.LoggerUtility.LogCondition(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
        return result;
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

