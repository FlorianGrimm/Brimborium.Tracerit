

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

    public override OnTraceResult OnTrace(
        TracorIdentitfier callee,
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<DataStepState>();
        if (state.Successfull) {
            return OnTraceResult.Successfull;
        }
        var childIndex = state.DataIndex;
        var count = this._Expected.ListData.Count;
        if (childIndex < count) {
            var childResult = IsPartialEquals(
                currentData: new TracorIdentitfierData(callee, tracorData),
                expectedData: this._Expected.ListData[childIndex],
                currentContext, state, childIndex);
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

    private bool IsPartialEquals(
        TracorIdentitfierData currentData,
        TracorDataRecord expectedData,
        OnTraceStepCurrentContext currentContext,
        DataStepState state,
        int childIndex) {
        var operation = expectedData.GetOperation();
        if (operation is TracorDataRecordOperation.Data or TracorDataRecordOperation.Unknown) {
            return IsPartialEqualsOperationData(
                currentData,
                expectedData,
                currentContext,
                state,
                childIndex);
        }

        if (operation is TracorDataRecordOperation.Filter) {
        }

        var count = this._Expected.ListData.Count;
        var childIndexArgument = childIndex + 1;
        this.GetOpArgumentData(childIndex + 1);
        TracorDataRecord opArgumentData;
        for (; childIndexArgument < count; childIndexArgument++) {
            opArgumentData = this._Expected.ListData[childIndexArgument];
            if (opArgumentData.GetOperation() == TracorDataRecordOperation.Data) { 
            }
        }
        if (count <= childIndexArgument) {
            // error
            // TODO: report
            return true;
        }
        var operationData = expectedData;
        if (operation is TracorDataRecordOperation.VariableGet) {
                opArgumentData
            foreach (var operationProperty in operationData.ListProperty) {
                var nameToRead = operationProperty.Name;
                var nameToWrite = operationProperty.TextValue is { Length:>0}? operationProperty.TextValue:nameToRead;
            }
        }
        if (operation is TracorDataRecordOperation.VariableSet) {
        }

        // no diff found
        return true;
    }

    private (TracorDataRecord? tdr, int index) GetOpArgumentData(int index) {
        var count = this._Expected.ListData.Count;
        int indexOp = index;
        for (; indexOp < count; indexOp++) {
        var    opArgumentData = this._Expected.ListData[indexOp];
            if (opArgumentData.GetOperation() == TracorDataRecordOperation.Data) {
                return (opArgumentData, indexOp);
            }
        }
        if (count <= indexOp) {
            // error
            // TODO: report
            return (null, indexOp);
        }
        return (null, index);
    }

    private static bool IsPartialEqualsOperationData(
        TracorIdentitfierData currentData,
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
                if (currentData.TracorData.TryGetPropertyValue(expectedProperty.Name, out var currentPropertyValue)) {
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
