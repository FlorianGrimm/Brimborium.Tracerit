namespace Brimborium.Tracerit.Condition;

public sealed class OrCondition : IExpressionCondition {
    private readonly IExpressionCondition[] _ExpressionConditions;

    public OrCondition(
        params IExpressionCondition[] expressionConditions) {
        this._ExpressionConditions = expressionConditions;
    }

    public IExpressionCondition[] ExpressionConditions => this._ExpressionConditions;

    public TracorValidatorOnTraceResult DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        for (int idx = 0; idx < this._ExpressionConditions.Length; idx++) {
            IExpressionCondition condition = this._ExpressionConditions[idx];
            TracorValidatorOnTraceResult result = condition.DoesMatch(tracorData, currentContext);
            if (result == TracorValidatorOnTraceResult.Successful) {
                return TracorValidatorOnTraceResult.Successful;
            } else if (result == TracorValidatorOnTraceResult.None) {
                if (this._ExpressionConditions.Length <= (idx + 1)) {
                    return TracorValidatorOnTraceResult.None;
                } else {
                    continue;
                }
            } else if (result == TracorValidatorOnTraceResult.None) {
                return TracorValidatorOnTraceResult.Failed;
            }
        }
        return TracorValidatorOnTraceResult.None;
    }

    public static OrCondition operator +(OrCondition left, IExpressionCondition right) {
        if (right is OrCondition orConditionRight) {
            return new OrCondition([left, .. orConditionRight.ExpressionConditions]);
        }

        return new OrCondition([.. left.ExpressionConditions, right]);
    }
}