namespace Brimborium.Tracerit.Condition;

public sealed class AndCondition : IExpressionCondition {
    private readonly IExpressionCondition[] _ExpressionConditions;

    public AndCondition(
        params IExpressionCondition[] expressionConditions) {
        this._ExpressionConditions = expressionConditions;
    }

    public IExpressionCondition[] ExpressionConditions => this._ExpressionConditions;

    public TracorValidatorOnTraceResult DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        for (int idx = 0; idx < this._ExpressionConditions.Length; idx++) {
            IExpressionCondition condition = this._ExpressionConditions[idx];
            TracorValidatorOnTraceResult result = condition.DoesMatch(tracorData, currentContext);
            if (result == TracorValidatorOnTraceResult.Successful) {
                if (this._ExpressionConditions.Length <= (idx + 1)) {
                    return TracorValidatorOnTraceResult.Successful;
                } else {
                    continue;
                }
            } else if (result == TracorValidatorOnTraceResult.Failed) {
                return TracorValidatorOnTraceResult.Failed;
            } else{
                return TracorValidatorOnTraceResult.None;
            }
        }
        return TracorValidatorOnTraceResult.Successful;
    }

    public static AndCondition operator *(AndCondition left, IExpressionCondition right) {
        if (right is AndCondition orConditionRight) {
            return new AndCondition([left, .. orConditionRight.ExpressionConditions]);
        }
        return new AndCondition([.. left.ExpressionConditions, right]);
    }
}
