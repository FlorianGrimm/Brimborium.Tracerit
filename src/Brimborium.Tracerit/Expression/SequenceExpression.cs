namespace Brimborium.Tracerit.Expression;

public sealed class SequenceExpression : ValidatorExpression {
    private ImmutableArray<IValidatorExpression> _ListChild = ImmutableArray<IValidatorExpression>.Empty;

    public SequenceExpression(
        string? label = default,
        params IValidatorExpression[] listChild
        ) : base(label) {
        if (0 < listChild.Length) { this._ListChild = this._ListChild.AddRange(listChild); }
    }

    public SequenceExpression Add(
        IValidatorExpression step) {
        using (this._Lock.EnterScope()) {
            this._ListChild = this._ListChild.Add(step);
        }
        return this;
    }

    public override OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<SequenceStepState>();
        if (state.Successfull) {
            return OnTraceResult.Successfull;
        }

        var childIndex = state.ChildIndex;
        if (childIndex < this._ListChild.Length) {
            var childContext = currentContext.GetChildContext(childIndex);
            var childResult = this._ListChild[childIndex].OnTrace(callee, tracorData, childContext);
            if (OnTraceResult.Successfull == childResult) {
                childIndex++;
                if (childIndex < this._ListChild.Length) {
                    state.ChildIndex = childIndex;
                    return OnTraceResult.None;
                } else {
                    state.ChildIndex = this._ListChild.Length;
                    currentContext.SetStateSuccessfull(this, state);
                    return OnTraceResult.Successfull;
                }
            } else {
                return OnTraceResult.None;
            }
        } else {
            currentContext.SetStateSuccessfull(this, state);
            return OnTraceResult.Successfull;
        }
    }

    internal sealed class SequenceStepState : ValidatorExpressionState {
        public int ChildIndex = 0;
    }
}
