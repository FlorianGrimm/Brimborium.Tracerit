namespace OpenTelemetry.Exporter.Tracerit;

public sealed class TracorTraceExportProcessor : SimpleExportProcessor<Activity> {
    /// <summary>
    /// Initializes a new instance of the <see cref="TracorTraceExportProcessor"/> class.
    /// </summary>
    /// <param name="exporter"><inheritdoc cref="SimpleExportProcessor{T}.SimpleExportProcessor" path="/param[@name='exporter']"/>.</param>
    public TracorTraceExportProcessor(TracorTraceExporter exporter)
        : base(exporter) {
    }

    /// <inheritdoc />
    public override void OnEnd(Activity data) {
        ArgumentNullException.ThrowIfNull(data);
#pragma warning disable CA1062 // Validate arguments of public methods - needed for netstandard2.1
        if (!data.Recorded)
#pragma warning restore CA1062 // Validate arguments of public methods - needed for netstandard2.1
        {
            return;
        }

        this.OnExport(data);
    }
}
