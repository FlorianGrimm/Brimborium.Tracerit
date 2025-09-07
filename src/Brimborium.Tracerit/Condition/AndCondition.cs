namespace Brimborium.Tracerit.Condition;

public sealed class AndCondition : IExpressionCondition {
    private readonly IExpressionCondition[] _ExpressionConditions;

    public AndCondition(
        params IExpressionCondition[] expressionConditions) {
        this._ExpressionConditions = expressionConditions;
    }

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        foreach (var condition in _ExpressionConditions) {
            if (condition.DoesMatch(callee, tracorData, currentContext)) {
                continue;
            } else { 
                return false;
            }
        }
        return true;
    }
}
