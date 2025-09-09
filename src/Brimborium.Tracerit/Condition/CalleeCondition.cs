namespace Brimborium.Tracerit.Condition;

public sealed class CalleeCondition : IExpressionCondition {
    private readonly TracorIdentitfier _Expected;
    private readonly IExpressionCondition? _And;

    public CalleeCondition(
        TracorIdentitfier expected,
        IExpressionCondition? and = default) {
        this._Expected = expected;
        this._And = and;
    }

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        bool resultCallee = this._Expected.Equals(callee);
        currentContext.LoggerUtility.LogCondition(callee, resultCallee, callee.ToString());
        if (resultCallee) {
            if (this._And is { } and) {
                var resultCondition = and.DoesMatch(callee, tracorData, currentContext);
                return resultCondition;
            } else {
                return true;
            }
        } else {
            return false;
        }
    }
}
