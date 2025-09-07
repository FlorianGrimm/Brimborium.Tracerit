namespace Brimborium.Tracerit.Expression;

public sealed class MatchExpression : ValidatorExpression {
    private ImmutableArray<IValidatorExpression> _ListChild = ImmutableArray<IValidatorExpression>.Empty;

    public MatchExpression(
        string? label = default,
        IExpressionCondition? condition = default,
        params IValidatorExpression[] listChild
        ) : base(label) {
        if (condition is { }) { this.Condition = condition; }
        if (0 < listChild.Length) { this._ListChild = listChild.ToImmutableArray(); }
    }

    public MatchExpression Add(
       IValidatorExpression step) {
        using (this._Lock.EnterScope()) {
            this._ListChild = this._ListChild.Add(step);
        }
        return this;
    }

    public IExpressionCondition Condition { get; set; } = AlwaysCondition.Instance;

    public override OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<MatchStepState>();
        if (state.Successfull) {
            return OnTraceResult.Successfull;
        }
        if (!state.Matched) {
            var matched = this.Condition.DoesMatch(callee, tracorData, currentContext);
            if (matched) {
                state.Matched = true;
                if (0 == this._ListChild.Length) {
                    currentContext.SetStateSuccessfull(this, state);
                    return OnTraceResult.Successfull;
                } else {
                    return OnTraceResult.None;
                }
            } else {
                return OnTraceResult.None;
            }
        }
        {
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
    }

    internal sealed class MatchStepState : ValidatorExpressionState {
        public bool Matched;
        public int ChildIndex;
    }
}
