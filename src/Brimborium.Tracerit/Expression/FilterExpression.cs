namespace Brimborium.Tracerit.Expression;

/// <summary>
/// Represents a validator expression that filters trace data based on a condition and processes all child expressions simultaneously.
/// Unlike sequence expressions, all child expressions are evaluated for each matching trace event.
/// </summary>
public sealed class FilterExpression : ValidatorExpression {
    private ImmutableArray<IValidatorExpression> _ListChild = ImmutableArray<IValidatorExpression>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterExpression"/> class.
    /// </summary>
    /// <param name="label">Optional label for this filter expression.</param>
    /// <param name="condition">The condition that trace data must match. If null, defaults to <see cref="AlwaysCondition"/>.</param>
    /// <param name="listChild">The child validator expressions to process when the condition matches.</param>
    public FilterExpression(
        string? label = default,
        IExpressionCondition? condition = default,
        params IValidatorExpression[] listChild)
        : base(label) {
        if (condition is { }) { this.Condition = condition; }
        if (0 < listChild.Length) { this._ListChild = listChild.ToImmutableArray(); }
    }

    /// <summary>
    /// Adds a validator expression to the list of child expressions.
    /// </summary>
    /// <param name="step">The validator expression to add.</param>
    /// <returns>This <see cref="FilterExpression"/> instance for method chaining.</returns>
    public FilterExpression Add(
       IValidatorExpression step) {
        using (this._Lock.EnterScope()) {
            this._ListChild = this._ListChild.Add(step);
        }
        return this;
    }

    /// <summary>
    /// Gets or sets the condition that trace data must match for this expression to process child expressions.
    /// </summary>
    public IExpressionCondition Condition { get; set; } = AlwaysCondition.Instance;

    /// <summary>
    /// Processes a trace event by checking if it matches the condition, then evaluating all child expressions.
    /// The expression succeeds when all child expressions have been successfully matched.
    /// </summary>
    /// <param name="callee">The identifier of the caller or trace point.</param>
    /// <param name="tracorData">The trace data to validate.</param>
    /// <param name="currentContext">The current context of the validation step.</param>
    /// <returns>The result of the trace validation.</returns>
    public override TracorValidatorOnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<FilterExpressionState>();
        if (state.Result.IsComplete()) {
            return state.Result;
        }

        var conditionResult = this.Condition.DoesMatch(tracorData, currentContext);
        if (TracorValidatorOnTraceResult.Successful == conditionResult) {
            for (var idx = 0; idx < this._ListChild.Length; idx++) {
                var child = this._ListChild[idx];
                var childResult = child.OnTrace(tracorData, currentContext.GetChildContext(idx));
                if (TracorValidatorOnTraceResult.Successful == childResult) {
                    state.ChildSuccessful.Add(idx);
                }
            }
            if (state.ChildSuccessful.Count == this._ListChild.Length) {
                return currentContext.SetStateSuccessful(this, state, tracorData.Timestamp);
            }
        }
        return TracorValidatorOnTraceResult.None;
    }

    /// <summary>
    /// Internal state class for tracking which child expressions have been successfully matched.
    /// </summary>
    internal sealed class FilterExpressionState : ValidatorExpressionState {
        /// <summary>
        /// Gets or sets the set of child expression indices that have been successfully matched.
        /// </summary>
        public HashSet<int> ChildSuccessful;

        public FilterExpressionState() {
            this.ChildSuccessful = new();
        }

        private FilterExpressionState(
            TracorValidatorOnTraceResult result,
            HashSet<int> childSuccessful) {
            this.Result = result;
            this.ChildSuccessful = childSuccessful;
        }

        protected internal override ValidatorExpressionState Copy()
            => new FilterExpressionState(
                this.Result,
                this.ChildSuccessful.ToHashSet());
    }
}

#if false
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

    public override OnTraceResult OnTrace(TracorIdentifier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
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

#endif

public static class FilterExpressionExtension {
    public static FilterExpression FilterExpression(
        this IExpressionCondition condition,
        string? label = default,
        params IValidatorExpression[] listChild
        )
        => new(label, condition, listChild);

#if false
    public static FilterExpression<T> FilterExpression<T>(
        this IExpressionCondition<T> condition,
        string? label = default,
        params IValidatorExpression[] listChild
        ) 
        => new(label, condition, listChild);
#endif
}