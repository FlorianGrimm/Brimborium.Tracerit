namespace Brimborium.Tracerit.Condition;

public sealed class PredicateTracorDataCondition : IExpressionCondition {
    private readonly Func<ITracorData, bool> _FnCondition;
    private readonly string? _FnConditionDisplay;

    public PredicateTracorDataCondition(
        Func<ITracorData, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition = fnCondition;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }

    public bool DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        bool result = this._FnCondition(tracorData);
        currentContext.LoggerUtility.LogCondition(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
        return result;
    }

    public static OrCondition operator +(PredicateTracorDataCondition left, IExpressionCondition right) {
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateTracorDataCondition left, IExpressionCondition right) {
        return new AndCondition([left, right]);
    }
}

public sealed class PredicateTracorDataCondition<TTracorData> : IExpressionCondition
    where TTracorData : ITracorData {
    private readonly Func<TTracorData, bool> _FnCondition;
    private readonly string? _FnConditionDisplay;

    public PredicateTracorDataCondition(
        Func<TTracorData, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) {
        this._FnCondition = fnCondition;
        this._FnConditionDisplay = doNotPopulateThisValue;
    }
    public bool DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is TTracorData tracorDataTyped) {
            var result = this._FnCondition(tracorDataTyped);
            currentContext.LoggerUtility.LogCondition(tracorData.TracorIdentifier, result, this._FnConditionDisplay);
            return result;
        }
        return false;
    }

    public static OrCondition operator +(PredicateTracorDataCondition<TTracorData> left, IExpressionCondition right) {
        return new OrCondition([left, right]);
    }

    public static AndCondition operator *(PredicateTracorDataCondition<TTracorData> left, IExpressionCondition right) {
        return new AndCondition([left, right]);
    }
}