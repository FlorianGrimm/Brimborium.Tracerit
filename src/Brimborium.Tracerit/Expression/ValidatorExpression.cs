namespace Brimborium.Tracerit.Expression;

/// <summary>
/// Abstract base class for validator expressions that implement <see cref="IValidatorExpression"/>.
/// Provides common functionality including instance indexing, labeling, and thread-safe operations.
/// </summary>
public abstract class ValidatorExpression : IValidatorExpression {
    protected readonly Lock _Lock = new();
    private static int _InstanceCount = 0;
    protected readonly int _InstanceIndex;

    /// <inheritdoc />
    int IValidatorExpression.GetInstanceIndex() => this._InstanceIndex;

    /// <inheritdoc />
    public string? Label { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorExpression"/> class.
    /// </summary>
    /// <param name="label">Optional label for this validator expression.</param>
    protected ValidatorExpression(string? label = default) {
        this._InstanceIndex = Interlocked.Increment(ref _InstanceCount);
        this.Label = label;
    }

    /// <inheritdoc />
    public abstract OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext onTraceStepCurrentContext);
}