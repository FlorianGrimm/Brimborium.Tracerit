using OpenTelemetry.Logs;

namespace OpenTelemetry.Exporter.Tracerit;

/// <summary>
/// Implements a simple log record export processor.
/// </summary>
public sealed class TracorLogRecordExportProcessor : SimpleExportProcessor<LogRecord> {
    /// <summary>
    /// Initializes a new instance of the <see cref="TracorLogRecordExportProcessor"/> class.
    /// </summary>
    /// <param name="exporter">Log record exporter.</param>
    public TracorLogRecordExportProcessor(BaseExporter<LogRecord> exporter)
        : base(exporter) {
    }
}
