namespace Brimborium.Tracerit.Expression;

public sealed class RecordExpression : ValidatorExpression {
    private readonly RecordExpressionResult _ReportExpressionResult;
    private readonly IValidatorExpression _Child;

    public RecordExpression(RecordExpressionResult reportExpressionResult, IValidatorExpression child) {
        this._ReportExpressionResult = reportExpressionResult;
        this._Child = child;
    }

    public override TracorValidatorOnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        if (tracorData is TracorDataRecord tracorDataRecord) {
            tracorDataRecord.IncrementReferenceCount();
            this._ReportExpressionResult.ListData.Add(tracorData);
        } else {
            tracorDataRecord = TracorDataRecord.Convert(tracorData, null);
            this._ReportExpressionResult.ListData.Add(tracorDataRecord);
            tracorData = tracorDataRecord;
        }

        var childResult = this._Child.OnTrace(tracorData, currentContext.GetChildContext(0));
        return childResult;
    }

    internal sealed class ReportExpressionState : ValidatorExpressionState {
        public ReportExpressionState() {
        }

        private ReportExpressionState(
            TracorValidatorOnTraceResult result
            ) : base(result) {
        }

        protected internal override ValidatorExpressionState Copy()
            => new ReportExpressionState(this.Result);
    }
}

public sealed class RecordExpressionResult : IDisposable {

    public List<ITracorData> ListData { get; } = new();

    public TracorDataCollection ToTracorListData()
        => new TracorDataCollection(this.ListData);

    public string ToTracorDataCollectionJson(
        System.Text.Json.JsonSerializerOptions? options = null)
        => TracorDataSerialization.SerializeSimple(
            this.ListData, options);

    private void Dispose(bool disposing) {
        if (0 < this.ListData.Count) {
            foreach (var item in this.ListData) {
                if (item is IReferenceCountObject referenceCountObject) {
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