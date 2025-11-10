namespace Brimborium.Tracerit.Expression;

/// <summary>
/// Represents a validator expression that matches trace data against a condition and then processes child expressions.
/// The expression first checks if the trace data matches the specified condition, and if so, processes child expressions in sequence.
/// </summary>
public sealed class MatchExpression : ValidatorExpression {
    private ImmutableArray<IValidatorExpression> _ListChild = ImmutableArray<IValidatorExpression>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MatchExpression"/> class.
    /// </summary>
    /// <param name="label">Optional label for this match expression.</param>
    /// <param name="condition">The condition that trace data must match. If null, defaults to <see cref="AlwaysCondition"/>.</param>
    /// <param name="listChild">The child validator expressions to process after a successful match.</param>
    public MatchExpression(
        string? label = default,
        IExpressionCondition? condition = default,
        params IValidatorExpression[] listChild
        ) : base(label) {
        if (condition is { }) { this.Condition = condition; }
        if (0 < listChild.Length) { this._ListChild = listChild.ToImmutableArray(); }
    }

    /// <summary>
    /// Adds a validator expression to the list of child expressions.
    /// </summary>
    /// <param name="step">The validator expression to add.</param>
    /// <returns>This <see cref="MatchExpression"/> instance for method chaining.</returns>
    public MatchExpression Add(
       IValidatorExpression step) {
        using (this._Lock.EnterScope()) {
            this._ListChild = this._ListChild.Add(step);
        }
        return this;
    }

    /// <summary>
    /// Gets or sets the condition that trace data must match for this expression to succeed.
    /// </summary>
    public IExpressionCondition Condition { get; set; } = AlwaysCondition.Instance;

    /// <summary>
    /// Processes a trace event by first checking if it matches the condition, then processing child expressions in sequence.
    /// </summary>
    /// <param name="tracorData">The trace data to validate.</param>
    /// <param name="currentContext">The current context of the validation step.</param>
    /// <returns>The result of the trace validation.</returns>
    public override TracorValidatorOnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<MatchStepState>();
        if (state.Result.IsComplete()) {
            return state.Result;
        }
        if (!state.Matched) {
            var conditionResult = this.Condition.DoesMatch(tracorData, currentContext);
            if (TracorValidatorOnTraceResult.Successful == conditionResult) {
                state.Matched = true;
                if (0 == this._ListChild.Length) {
                    return currentContext.SetStateSuccessful(this, state, tracorData.Timestamp);
                } else {
                    return TracorValidatorOnTraceResult.None;
                }
            } else if (TracorValidatorOnTraceResult.Failed == conditionResult) {
                return currentContext.SetStateFailed(this, state, tracorData.Timestamp);
            } else { 
                return TracorValidatorOnTraceResult.None;
            }
        }
        {
            var childIndex = state.ChildIndex;
            if (childIndex < this._ListChild.Length) {
                var childContext = currentContext.GetChildContext(childIndex);
                var childResult = this._ListChild[childIndex].OnTrace(tracorData, childContext);
                if (TracorValidatorOnTraceResult.Successful == childResult) {
                    childIndex++;
                    if (childIndex < this._ListChild.Length) {
                        state.ChildIndex = childIndex;
                        return TracorValidatorOnTraceResult.None;
                    } else {
                        state.ChildIndex = this._ListChild.Length;
                        return currentContext.SetStateSuccessful(this, state, tracorData.Timestamp);
                    }
                } else if (TracorValidatorOnTraceResult.Failed == childResult) {
                    return currentContext.SetStateFailed(this, state, tracorData.Timestamp);
                } else {
                    return TracorValidatorOnTraceResult.None;
                }
            } else {
                return currentContext.SetStateSuccessful(this, state, tracorData.Timestamp);
            }
        }
    }

    /// <summary>
    /// Internal state class for tracking the progress of a match expression.
    /// </summary>
    internal sealed class MatchStepState : ValidatorExpressionState {
        /// <summary>
        /// Gets or sets a value indicating whether the condition has been matched.
        /// </summary>
        public bool Matched;

        /// <summary>
        /// Gets or sets the index of the current child expression being processed.
        /// </summary>
        public int ChildIndex;

        public MatchStepState() {
        }

        private MatchStepState(
            TracorValidatorOnTraceResult result,
            bool matched,
            int childIndex
            ) : base(result) {
            this.Matched = matched;
            this.ChildIndex = childIndex;
        }

        protected internal override ValidatorExpressionState Copy()
            => new MatchStepState(this.Result, this.Matched, this.ChildIndex);
    }
}
