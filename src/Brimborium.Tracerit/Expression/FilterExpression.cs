namespace Brimborium.Tracerit.Expression;

public sealed class FilterExpression<T> : ValidatorExpression {
    private ImmutableArray<IValidatorExpression> _ListChild = ImmutableArray<IValidatorExpression>.Empty;

    public FilterExpression(
        string? label = default,
        IExpressionCondition? condition = default,
        params IValidatorExpression[] listChild)
        : base(label) {
        if (condition is { }) { this.Condition = condition; }
        if (0 < listChild.Length) { this._ListChild = listChild.ToImmutableArray(); }
    }

    public FilterExpression<T> Add(
       IValidatorExpression step) {
        using (this._Lock.EnterScope()) {
            this._ListChild = this._ListChild.Add(step);
        }
        return this;
    }

    public IExpressionCondition Condition { get; set; } = AlwaysCondition.Instance;

    public override OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<FilterExpressionState>();
        if (state.Successfull) {
            return OnTraceResult.Successfull;
        }

        if (this.Condition.DoesMatch(callee, tracorData, currentContext)) {
            for (var idx = 0; idx < this._ListChild.Length; idx++) {
                var child = this._ListChild[idx];
                var childResult = child.OnTrace(callee, tracorData, currentContext.GetChildContext(idx));
                if (OnTraceResult.Successfull == childResult) {
                    state.ChildSuccessfull.Add(idx);
                }
            }
            if (state.ChildSuccessfull.Count == this._ListChild.Length) {
                currentContext.SetStateSuccessfull(this, state);
                return OnTraceResult.Successfull;
            }
        }
        return OnTraceResult.None;
    }

    internal sealed class FilterExpressionState : ValidatorExpressionState {
        public HashSet<int> ChildSuccessfull = new();
    }
}

public static class FilterExpressionExtension{
    public static FilterExpression<T> FilterExpression<T>(
        this IExpressionCondition<T> condition,
        string? label = default,
        params IValidatorExpression[] listChild
        ) 
        => new FilterExpression<T>(label, condition, listChild);
}