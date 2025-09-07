namespace Brimborium.Tracerit.Expression;

public sealed class AllOfExpression : ValidatorExpression {
    private ImmutableArray<IValidatorExpression> _ListChild = ImmutableArray<IValidatorExpression>.Empty;

    public AllOfExpression(
        string? label = default,
        params IValidatorExpression[] listChild
        ) : base(label) {
        if (0 < listChild.Length) { this._ListChild = listChild.ToImmutableArray(); }
    }

    public AllOfExpression Add(
        IValidatorExpression step) {
        using (this._Lock.EnterScope()) {
            this._ListChild = this._ListChild.Add(step);
        }
        return this;
    }

    public override OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<OneOfExpressionState>();
        if (state.Successfull) {
            return OnTraceResult.Successfull;
        }
        for (var idx = 0; idx < this._ListChild.Length; idx++) {
            if (state.ChildSuccessfull.Contains(idx)) {
                // skip
            } else {
                var child = this._ListChild[idx];
                var childResult = child.OnTrace(callee, tracorData, currentContext.GetChildContext(idx));
                if (OnTraceResult.Successfull == childResult) {
                    state.ChildSuccessfull.Add(idx);
                    if (state.ChildSuccessfull.Count == this._ListChild.Length) {
                        currentContext.SetStateSuccessfull(this, state);
                        return OnTraceResult.Successfull;
                    }
                }
            }
        }
        return OnTraceResult.None;
    }

    internal sealed class OneOfExpressionState : ValidatorExpressionState {
        public HashSet<int> ChildSuccessfull = new();
    }
}