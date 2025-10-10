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

    public override OnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<OneOfExpressionState>();
        if (state.Successfull) {
            return OnTraceResult.Successfull;
        }
        for (var idx = 0; idx < this._ListChild.Length; idx++) {
            var child = this._ListChild[idx];
            var childResult = child.OnTrace(tracorData, currentContext.GetChildContext(idx));
            if (OnTraceResult.Successfull == childResult) {
                currentContext.SetStateSuccessfull(this, state);
                return OnTraceResult.Successfull;
            }
        }
        return OnTraceResult.None;
    }

    internal sealed class OneOfExpressionState : ValidatorExpressionState {
    }
}
