namespace Brimborium.Tracerit.Condition;

public sealed class OrCondition : IExpressionCondition {
    private readonly IExpressionCondition[] _ExpressionConditions;

    public OrCondition(
        params IExpressionCondition[] expressionConditions) {
        this._ExpressionConditions = expressionConditions;
    }

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        foreach (var condition in _ExpressionConditions) {
            if (condition.DoesMatch(callee, tracorData, currentContext)) {
                return true;
            } else {
                continue;
            }
        }
        return false;
    }
}