
namespace Brimborium.Tracerit.Expression;

public sealed class DataExpression : ValidatorExpression {
    public override OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext onTraceStepCurrentContext) {
        throw new NotImplementedException();
    }

    internal sealed class DataStepState : ValidatorExpressionState {
        public int DataIndex = 0;
    }
}
