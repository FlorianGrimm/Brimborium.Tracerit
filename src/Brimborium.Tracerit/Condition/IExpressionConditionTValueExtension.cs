namespace Brimborium.Tracerit.Condition;

public static class IExpressionConditionTValueExtension {
    public static IValidatorExpression AsMatch<TValue>(this IExpressionCondition<TValue> expressionCondition, string? label=default, params IValidatorExpression[] listChild) {
        return new MatchExpression(
            label: label,
            condition: expressionCondition,
            listChild: listChild);
    }

    public static IValidatorExpression AsMatch(this IExpressionCondition expressionCondition, string? label = default, params IValidatorExpression[] listChild) {
        return new MatchExpression(
            label: label,
            condition: expressionCondition,
            listChild: listChild);
    }
}