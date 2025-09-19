namespace Brimborium.Tracerit.Expression;

public sealed class RecordExpression : ValidatorExpression {
    private readonly RecordExpressionResult _ReportExpressionResult;
    private readonly IValidatorExpression _Child;

    public RecordExpression(RecordExpressionResult reportExpressionResult, IValidatorExpression child) {
        this._ReportExpressionResult = reportExpressionResult;
        this._Child = child;
    }

    public override OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is IReferenceCountObject referenceCountObject) {
            referenceCountObject.IncrementReferenceCount();
        }
        this._ReportExpressionResult.ListData.Add(new(callee, tracorData));
        var childResult = this._Child.OnTrace(callee, tracorData, currentContext.GetChildContext(0));
        return childResult;
    }

    internal sealed class ReportExpressionState : ValidatorExpressionState { }
}

public sealed class RecordExpressionResult : IDisposable {

    public List<TracorIdentitfierData> ListData { get; } = new();

    public TracorDataCollection ToTracorListData()
        => TracorDataSerialization.ToTracorDataCollection(this.ListData);

    public string ToTracorDataCollectionJson(
        System.Text.Json.JsonSerializerOptions? options = null)
        => TracorDataSerialization.ToTracorDataCollectionJson(this.ListData, options);

    private void Dispose(bool disposing) {
        if (0 < this.ListData.Count) {
            foreach (var item in this.ListData) {
                if (item.TracorData is IReferenceCountObject referenceCountObject) {
                    referenceCountObject.Dispose();
                }
            }
        }
        if (disposing) {
            this.ListData.Clear();
        }
    }

    ~RecordExpressionResult() {
        this.Dispose(disposing: false);
    }

    public void Dispose() {
        this.Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
}