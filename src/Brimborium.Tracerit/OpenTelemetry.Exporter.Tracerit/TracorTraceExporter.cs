namespace OpenTelemetry.Exporter.Tracerit;

public sealed class TracorTraceExporter : BaseExporter<Activity> {
    private readonly ITracor _Tracor;
    private readonly TracorIdentitfier _TracorIdentitfier;
    private bool _IsDisposed;
    private string? _DisposedStackTrace;

    public TracorTraceExporter(ITracor tracor) {
        this._Tracor = tracor;
        this._TracorIdentitfier = new TracorIdentitfier("OpenTelemetry.Trace");
    }

    public override ExportResult Export(in Batch<Activity> batch) {
        if (this._IsDisposed) {
            throw new ObjectDisposedException(
                this.GetType().Name,
                $"The Tracerit exporter is still being invoked after it has been disposed. Dispose was called on the following stack trace:{Environment.NewLine}{this._DisposedStackTrace}");
        }
        if (this._Tracor.IsGeneralEnabled()
            && this._Tracor.IsCurrentlyEnabled()) {
            foreach (var item in batch) {
                //TracorIdentitfier tracorIdentitfier = (item.TraceId.ToString() is { } traceId) ? new TracorIdentitfier(traceId) : this._TracorIdentitfier;
                this._Tracor.Trace(this._TracorIdentitfier, item);
            }
        }
        return ExportResult.Success;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing) {
        if (!this._IsDisposed) {
            this._DisposedStackTrace = Environment.StackTrace;
            this._IsDisposed = true;
        }

        base.Dispose(disposing);
    }
}
