namespace Brimborium.Tracerit.Condition;

public sealed class CalleeCondition : IExpressionCondition {
    private readonly TracorIdentifier _Expected;
    private readonly IExpressionCondition? _And;

    public CalleeCondition(
        TracorIdentifier expected,
        IExpressionCondition? and = default) {
        this._Expected = expected;
        this._And = and;
    }

    public TracorValidatorOnTraceResult DoesMatch(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        //bool resultCallee = this._Expected.Equals(callee);
        bool resultCallee = MatchEqualityComparerTracorIdentifier.Default.Equals(
            tracorData.TracorIdentifier, 
            this._Expected);
        currentContext.LoggerUtility.LogConditionBool(tracorData.TracorIdentifier, resultCallee, tracorData.TracorIdentifier.ToString());
        if (resultCallee) {
            if (!(this._And is { } and)) {
                return TracorValidatorOnTraceResult.Successful;
            } 
            
            {
                var resultCondition = and.DoesMatch(tracorData, currentContext);
                return resultCondition;
            }
        } else {
            return TracorValidatorOnTraceResult.None;
        }
    }
}
