using Microsoft.Extensions.Logging;

namespace Brimborium.Tracerit.Server;

/// <summary>
/// Service interface for receiving trace data over HTTP.
/// </summary>
public interface ITracorCollectorHttpService {
    /// <summary>
    /// Converts and pushes trace data from the request body stream.
    /// </summary>
    /// <param name="body">The request body stream containing trace data.</param>
    /// <param name="applicationName">the resource</param>
    /// <param name="requestAborted">Cancellation token for the request.</param>
    Task ConvertAndPush(Stream body, string? applicationName, CancellationToken requestAborted);

    /// <summary>
    /// Handles an HTTP POST request containing trace data.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the request.</param>
    Task HandlePostAsync(HttpContext httpContext, string? applicationName);
}

public sealed class TracorCollectorHttpService : ITracorCollectorHttpService {
    private readonly JsonSerializerOptions _JsonSerializerOptions;
    private readonly ITracorServerCollectorWrite[] _ListTracorCollector;
    private readonly ILogger<TracorCollectorHttpService> _Logger;
    private bool _ErrorLogged;

    public TracorCollectorHttpService(
        IEnumerable<ITracorServerCollectorWrite> tracorCollector,
        TracorDataRecordPool tracorDataRecordPool,
        ILogger<TracorCollectorHttpService> logger
        ) {
        this._JsonSerializerOptions = TracorDataSerialization.AddTracorDataMinimalJsonConverter(null, tracorDataRecordPool);
        this._ListTracorCollector = tracorCollector.ToArray();
        this._Logger = logger;
    }

    public async Task HandlePostAsync(Microsoft.AspNetCore.Http.HttpContext httpContext, string? applicationName) {
        httpContext.Response.StatusCode = 200;
        try {
            Stream body = httpContext.Request.Body;
            await this.ConvertAndPush(body, applicationName, httpContext.RequestAborted).ConfigureAwait(false);
            this._ErrorLogged = false;
        } catch (Exception error) {
            if (this._ErrorLogged) {
                // skip a void recursion (the 2cd time)
            } else {
                this._ErrorLogged = true;
                this._Logger.LogError(error, nameof(HandlePostAsync));
            }
        }
    }

    public async Task ConvertAndPush(Stream body, string? applicationName, CancellationToken requestAborted) {
        using (BrotliStream utf8Stream = new BrotliStream(body, CompressionMode.Decompress)) {
            using (SplitStream splitStream = new(utf8Stream, leaveOpen: true, chunkSize: 4096)) {
                while (await splitStream.MoveNextStreamAsync(requestAborted)) {
                    using (var tracorDataRecord = await System.Text.Json.JsonSerializer.DeserializeAsync<TracorDataRecord>(
                        splitStream, this._JsonSerializerOptions, requestAborted)) {
                        if (tracorDataRecord is { }) {
                            foreach (var tracorCollector in _ListTracorCollector) { 
                                tracorCollector.Push(tracorDataRecord, applicationName);
                            }
                        }
                    }
                }
            }
        }
    }
}
