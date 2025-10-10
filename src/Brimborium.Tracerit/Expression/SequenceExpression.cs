namespace Brimborium.Tracerit.Expression;

/// <summary>
/// Represents a validator expression that requires child expressions to be matched in sequential order.
/// Each child expression must be successfully matched before proceeding to the next one.
/// </summary>
public sealed class SequenceExpression : ValidatorExpression {
    private ImmutableArray<IValidatorExpression> _ListChild = ImmutableArray<IValidatorExpression>.Empty;

    public ImmutableArray<IValidatorExpression> ListChild { get => this._ListChild; set => this._ListChild = value; }

    private SequenceExpression(
        string? label,
        ImmutableArray<IValidatorExpression> listChild
        ) : base(label) {
        this._ListChild = listChild;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceExpression"/> class.
    /// </summary>
    /// <param name="label">Optional label for this sequence expression.</param>
    /// <param name="listChild">The child validator expressions that must be matched in sequence.</param>
    public SequenceExpression(
        string? label = default,
        params IValidatorExpression[] listChild
        ) : base(label) {
        if (0 < listChild.Length) { this._ListChild = this._ListChild.AddRange(listChild); }
    }

    /// <summary>
    /// Adds a validator expression to the end of the sequence.
    /// </summary>
    /// <param name="step">The validator expression to add.</param>
    /// <returns>This <see cref="SequenceExpression"/> instance for method chaining.</returns>
    public SequenceExpression Add(
        IValidatorExpression step) {
        using (this._Lock.EnterScope()) {
            this._ListChild = this._ListChild.Add(step);
        }
        return this;
    }

    public override OnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<SequenceStepState>();
        if (state.Successfull) {
            return OnTraceResult.Successfull;
        }

        var childIndex = state.ChildIndex;
        if (childIndex < this._ListChild.Length) {
            var childContext = currentContext.GetChildContext(childIndex);
            var childResult = this._ListChild[childIndex].OnTrace(tracorData, childContext);
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

    public static SequenceExpression operator +(SequenceExpression left, IValidatorExpression right) {
        return new(left.Label, left.ListChild.Add(right));
    }
}
