namespace Brimborium.Tracerit.Condition;

/// <summary>
/// TODO
/// </summary>
public static class IExpressionConditionTValueExtension {
    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="expressionCondition"></param>
    /// <param name="label"></param>
    /// <param name="listChild"></param>
    /// <returns></returns>
    public static IValidatorExpression AsMatch<TValue>(this IExpressionCondition<TValue> expressionCondition, string? label=default, params IValidatorExpression[] listChild) {
        return new MatchExpression(
            label: label,
            condition: expressionCondition,
            listChild: listChild);
    }

/// <summary>
/// TODO
/// </summary>
/// <param name="expressionCondition"></param>
/// <param name="label"></param>
/// <param name="listChild"></param>
/// <returns></returns>
    public static IValidatorExpression AsMatch(this IExpressionCondition expressionCondition, string? label = default, params IValidatorExpression[] listChild) {
        return new MatchExpression(
            label: label,
            condition: expressionCondition,
            listChild: listChild);
    }
}