namespace Brimborium.Tracerit.Expression;

/// <summary>
/// Represents a validator expression that requires all child expressions to be successfully matched.
/// All child expressions must be satisfied for this expression to be considered successful.
/// </summary>
public sealed class AllOfExpression : ValidatorExpression {
    private ImmutableArray<IValidatorExpression> _ListChild = ImmutableArray<IValidatorExpression>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllOfExpression"/> class.
    /// </summary>
    /// <param name="label">Optional label for this all-of expression.</param>
    /// <param name="listChild">The child validator expressions that must all be matched.</param>
    public AllOfExpression(
        string? label = default,
        params IValidatorExpression[] listChild
        ) : base(label) {
        if (0 < listChild.Length) { this._ListChild = listChild.ToImmutableArray(); }
    }

    /// <summary>
    /// Adds a validator expression to the collection of expressions that must all be matched.
    /// </summary>
    /// <param name="step">The validator expression to add.</param>
    /// <returns>This <see cref="AllOfExpression"/> instance for method chaining.</returns>
    public AllOfExpression Add(
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
            if (state.ChildSuccessful.Contains(idx)) {
                // skip
            } else {
                var child = this._ListChild[idx];
                var childResult = child.OnTrace(tracorData, currentContext.GetChildContext(idx));
                if (TracorValidatorOnTraceResult.Successful == childResult) {
                    state.ChildSuccessful.Add(idx);
                    if (state.ChildSuccessful.Count == this._ListChild.Length) {
                        currentContext.SetStateSuccessful(this, state);
                        return TracorValidatorOnTraceResult.Successful;
                    }
                }
            }
        }
        return TracorValidatorOnTraceResult.None;
    }

    internal sealed class OneOfExpressionState : ValidatorExpressionState {
        public HashSet<int> ChildSuccessful;

        public OneOfExpressionState() {
            this.ChildSuccessful = new();
        }

        private OneOfExpressionState(
            TracorValidatorOnTraceResult result, 
            HashSet<int> childSuccessful) {
            this.Result = result;
            this.ChildSuccessful = childSuccessful;
        }

        protected internal override ValidatorExpressionState Copy() {
            return new OneOfExpressionState(
                this.Result,
                this.ChildSuccessful.ToHashSet());
        }
    }
}