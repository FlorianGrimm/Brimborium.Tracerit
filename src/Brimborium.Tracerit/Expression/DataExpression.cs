

namespace Brimborium.Tracerit.Expression;

public sealed class DataExpression : ValidatorExpression {
    private readonly TracorDataRecordCollection _Expected;

    public DataExpression(
        string expected
        ) {
        this._Expected = TracorDataSerialization.DeserializeSimple(expected);
    }

    public DataExpression(
        TracorDataRecordCollection expected
        ) {
        this._Expected = expected;
    }

    public override TracorValidatorOnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<DataStepState>();
        if (state.Result.IsComplete()) {
            return state.Result;
        }
        // TODO: here
        var childIndex = state.DataIndex;
        var count = this._Expected.ListData.Count;
        if (childIndex < count) {
            var childResult = this.IsPartialEquals(
                currentData: tracorData,
                expectedData: this._Expected.ListData[childIndex],
                currentContext, state, childIndex);
            if (childResult) {
                childIndex++;
                if (childIndex < count) {
                    state.DataIndex = childIndex;
                    return TracorValidatorOnTraceResult.None;
                } else {
                    state.DataIndex = count;
                    currentContext.SetStateSuccessfull(this, state);
                    return TracorValidatorOnTraceResult.Successfull;
                }
            } else {
                return TracorValidatorOnTraceResult.None;
            }
        } else {
            currentContext.SetStateSuccessfull(this, state);
            return TracorValidatorOnTraceResult.Successfull;
        }
    }

    private bool IsPartialEquals(
        ITracorData currentData,
        TracorDataRecord expectedData,
        OnTraceStepCurrentContext currentContext,
        DataStepState state,
        int childIndex) {

        if (expectedData.TracorIdentitfier is { } expectedtracorIdentitfier) {
            var currentTracorIdentitfier = currentData.TracorIdentitfier;
            if (!MatchEqualityComparerTracorIdentitfier.Default.Equals(
                    currentTracorIdentitfier,
                    expectedtracorIdentitfier)) {
                return false;
            }
        }
        if (0 < expectedData.ListProperty.Count) {
            foreach (var expectedProperty in expectedData.ListProperty) {
                if (currentData.TryGetPropertyValue(expectedProperty.Name, out var currentPropertyValue)) {
                    if (expectedProperty.HasEqualValue(currentPropertyValue)) {
                        // equal -> ok
                    } else {
                        // not equal
                        return false;
                    }
                } else {
                    // not found
                    return false;
                }
            }
        }
        // no diff found
        return true;
    }
}
internal sealed class DataStepState : ValidatorExpressionState {
    public int DataIndex = 0;
}
