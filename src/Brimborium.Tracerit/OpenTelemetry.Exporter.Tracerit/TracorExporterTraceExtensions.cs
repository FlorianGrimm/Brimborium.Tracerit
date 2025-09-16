using System.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Exporter.Tracerit;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Trace;

public static class TracorExporterTraceExtensions {
    /// <summary>
    /// Adds InMemory exporter to the TracerProvider.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> builder to use.</param>
    /// <param name="exportedItems">Collection which will be populated with the exported <see cref="Activity"/>.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddTraceritExporter(this TracerProviderBuilder builder) {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureServices(services => {
            services.AddSingleton<TracorTraceExporter>();
        });

        builder.AddProcessor<TracorTraceExportProcessor>();
        return builder;
    }
}

