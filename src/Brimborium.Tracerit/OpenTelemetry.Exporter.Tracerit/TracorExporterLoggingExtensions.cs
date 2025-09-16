using OpenTelemetry.Exporter;
using OpenTelemetry.Exporter.Tracerit;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Logs;

public static class TracorExporterLoggingExtensions {
    /// <summary>
    /// Adds Tracor exporter to the LoggerProviderBuilder.
    /// </summary>
    /// <param name="loggerProviderBuilder"><see cref="LoggerProviderBuilder"/>.</param>
    /// <returns>The supplied instance of <see cref="LoggerProviderBuilder"/> to chain the calls.</returns>
    public static LoggerProviderBuilder AddTracorExporter(
        this LoggerProviderBuilder loggerProviderBuilder,
        ICollection<LogRecord> exportedItems) {
        Guard.ThrowIfNull(loggerProviderBuilder);
        loggerProviderBuilder.ConfigureServices(services => {
            services.AddSingleton<TracorLogRecordExporter>();
        });
        loggerProviderBuilder.AddProcessor<TracorLogRecordExportProcessor>();
        return loggerProviderBuilder;
    }
}
