
namespace Brimborium.Tracerit.Expression;

public sealed class DataExpression : ValidatorExpression {
    private readonly TracorDataCollection _Expected;


    public DataExpression(
        string expected
        ) {
        this._Expected = TracorDataSerialization.ParseTracorDataCollection(expected);
    }

    public DataExpression(
        TracorDataCollection expected
        ) {
        this._Expected = expected;
    }

    public override OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<DataStepState>();
        if (state.Successfull) {
            return OnTraceResult.Successfull;
        }
        var childIndex = state.DataIndex;
        var count = this._Expected.ListData.Count;
        if (childIndex < count) {
            var childResult = TracorDataRecord.IsPartialEquals(new TracorIdentitfierData(callee, tracorData), this._Expected.ListData[childIndex]);
            if (childResult) {
                childIndex++;
                if (childIndex < count) {
                    state.DataIndex = childIndex;
                    return OnTraceResult.None;
                } else {
                    state.DataIndex = count;
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

    internal sealed class DataStepState : ValidatorExpressionState {
        public int DataIndex = 0;
    }
}
