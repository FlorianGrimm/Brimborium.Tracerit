namespace Brimborium.Tracerit.Condition;

public sealed class PredicateValueCondition<TValue> : IExpressionCondition<TValue> {
    private readonly Func<TValue, bool> _FnCondition;
    private readonly string? _FnConditionDisplay;

    public PredicateValueCondition(
        Func<TValue, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition = fnCondition;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public bool DoesMatch(
        ITracorData tracorData, 
        OnTraceStepCurrentContext currentContext) {
        if (tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var value)) {
            bool result = this._FnCondition(value);
            currentContext.LoggerUtility.LogCondition(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
            return result;
        }
        return false;
    }

    public static OrCondition operator +(PredicateValueCondition<TValue> left, IExpressionCondition right) {
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateValueCondition<TValue> left, IExpressionCondition right) {
        return new AndCondition([left, right]);
    }
}
