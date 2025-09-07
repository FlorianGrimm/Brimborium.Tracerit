namespace Brimborium.Tracerit.Expression;

public abstract class ValidatorExpression : IValidatorExpression {
    protected readonly Lock _Lock = new();
    private static int _InstanceCount = 0;
    protected readonly int _InstanceIndex;
    int IValidatorExpression.GetInstanceIndex() => this._InstanceIndex;

    public string? Label { get; set; }

    protected ValidatorExpression(string? label = default) {
        this._InstanceIndex = Interlocked.Increment(ref _InstanceCount);
        this.Label = label;
    }

    public abstract OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext onTraceStepCurrentContext);
}