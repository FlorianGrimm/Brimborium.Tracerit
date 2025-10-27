namespace Brimborium.Tracerit.Expression;

public sealed class OneOfExpression : ValidatorExpression {
    private ImmutableArray<IValidatorExpression> _ListChild = ImmutableArray<IValidatorExpression>.Empty;

    public OneOfExpression(
        string? label = default,
        params IValidatorExpression[] listChild
        ) : base(label) {
        if (0 < listChild.Length) { this._ListChild = listChild.ToImmutableArray(); }
    }

    public OneOfExpression Add(
        IValidatorExpression step) {
        using (this._Lock.EnterScope()) {
            this._ListChild = this._ListChild.Add(step);
        }
        return this;
    }

    public override TracorValidatorOnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<OneOfExpressionState>();
        if (state.Result.IsComplete()) {
            return state.Result;
        }
        for (var idx = 0; idx < this._ListChild.Length; idx++) {
            var child = this._ListChild[idx];
            var childResult = child.OnTrace(tracorData, currentContext.GetChildContext(idx));
            if (TracorValidatorOnTraceResult.Successful == childResult) {
                currentContext.SetStateSuccessful(this, state);
                return TracorValidatorOnTraceResult.Successful;
            }
        }
        return TracorValidatorOnTraceResult.None;
    }

    internal sealed class OneOfExpressionState : ValidatorExpressionState {
    }
}
