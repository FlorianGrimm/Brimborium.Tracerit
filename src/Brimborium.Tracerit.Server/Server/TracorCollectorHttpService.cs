using System.IO.Compression;
using System.Text.Json;

namespace Brimborium.Tracerit.Server;

public class TracorCollectorHttpService {
    private readonly JsonSerializerOptions _JsonSerializerOptions;
    private readonly ITracorCollector _TracorCollector;

    public TracorCollectorHttpService(
        ITracorCollector tracorCollector,
        TracorDataRecordPool tracorDataRecordPool
        ) {
        this._JsonSerializerOptions = JsonSerializerOptionsExtensions.AddTracorDataMinimalJsonConverter(null, tracorDataRecordPool);
        this._TracorCollector = tracorCollector;
    }

    public async Task HandlePostAsync(Microsoft.AspNetCore.Http.HttpContext httpContext) {
        httpContext.Response.StatusCode = 200;
        using (BrotliStream utf8Stream = new BrotliStream(httpContext.Request.Body, CompressionMode.Decompress)) {
            using (SplitStream splitStream = new(utf8Stream, leaveOpen: true, chunkSize: 4096)) {
                while (await splitStream.MoveNextStreamAsync(httpContext.RequestAborted)) {
                    using (var tracorDataRecord = await System.Text.Json.JsonSerializer.DeserializeAsync<TracorDataRecord>(
                        splitStream, this._JsonSerializerOptions, httpContext.RequestAborted)) {
                        if (tracorDataRecord is { }) {
                            this._TracorCollector.Push(tracorDataRecord);
                        }
                    }
                }
            }
        }
        //
        await Task.CompletedTask;
    }

    public void X() {
        //global::OpenTelemetry.Proto.Collector.Logs.V1.LogsService
    }

}
