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
        if (this._Expected.Equals(callee)) {
            if (this._And is { } and) {
                return and.DoesMatch(callee, tracorData, currentContext);
            } else {
                return true;
            }
        }
        return false;
    }
}
