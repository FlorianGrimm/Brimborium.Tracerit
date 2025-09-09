namespace Brimborium.Tracerit.Condition;

public sealed class AndCondition : IExpressionCondition {
    private readonly IExpressionCondition[] _ExpressionConditions;

    public AndCondition(
        params IExpressionCondition[] expressionConditions) {
        this._ExpressionConditions = expressionConditions;
    }

    public IExpressionCondition[] ExpressionConditions => this._ExpressionConditions;

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        for (int idx = 0; idx < this._ExpressionConditions.Length; idx++) {
            IExpressionCondition condition = this._ExpressionConditions[idx];
            bool result = condition.DoesMatch(callee, tracorData, currentContext);
            if (result) {
                if (this._ExpressionConditions.Length <= (idx + 1)) {
                    return true;
                } else {
                    continue;
                }
            } else {
                return false;
            }
        }
        return true;
    }

    public static AndCondition operator *(AndCondition left, IExpressionCondition right) {
        return new AndCondition([.. left.ExpressionConditions, right]);
    }
}
