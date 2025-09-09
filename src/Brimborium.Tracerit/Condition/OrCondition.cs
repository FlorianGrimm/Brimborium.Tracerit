namespace Brimborium.Tracerit.Condition;

public sealed class OrCondition : IExpressionCondition {
    private readonly IExpressionCondition[] _ExpressionConditions;

    public OrCondition(
        params IExpressionCondition[] expressionConditions) {
        this._ExpressionConditions = expressionConditions;
    }

    public IExpressionCondition[] ExpressionConditions => this._ExpressionConditions;

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        for (int idx = 0; idx < this._ExpressionConditions.Length; idx++) {
            IExpressionCondition condition = this._ExpressionConditions[idx];
            bool result = condition.DoesMatch(callee, tracorData, currentContext);
            if (result) {
                return true;
            } else {
                if (this._ExpressionConditions.Length <= (idx + 1)) {
                    return false;
                } else { 
                    continue;
                }
            }
        }
        return false;
    }

    public static OrCondition operator +(OrCondition left, IExpressionCondition right) {
        return new OrCondition([.. left.ExpressionConditions, right]);
    }
}