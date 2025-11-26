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
    /// <param name="requestAborted">Cancellation token for the request.</param>
    Task ConvertAndPush(Stream body, CancellationToken requestAborted);

    /// <summary>
    /// Handles an HTTP POST request containing trace data.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the request.</param>
    Task HandlePostAsync(HttpContext httpContext);
}

public sealed class TracorCollectorHttpService : ITracorCollectorHttpService {
    private readonly JsonSerializerOptions _JsonSerializerOptions;
    private readonly ITracorCollector _TracorCollector;
    private readonly ILogger<TracorCollectorHttpService> _Logger;
    private bool _ErrorLogged;

    public TracorCollectorHttpService(
        ITracorCollector tracorCollector,
        TracorDataRecordPool tracorDataRecordPool,
        ILogger<TracorCollectorHttpService> logger
        ) {
        this._JsonSerializerOptions = TracorDataSerialization.AddTracorDataMinimalJsonConverter(null, tracorDataRecordPool);
        this._TracorCollector = tracorCollector;
        this._Logger = logger;
    }

    public async Task HandlePostAsync(Microsoft.AspNetCore.Http.HttpContext httpContext) {
        httpContext.Response.StatusCode = 200;
        try {
            Stream body = httpContext.Request.Body;
            await this.ConvertAndPush(body, httpContext.RequestAborted).ConfigureAwait(false);
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

    public async Task ConvertAndPush(Stream body, CancellationToken requestAborted) {
        using (BrotliStream utf8Stream = new BrotliStream(body, CompressionMode.Decompress)) {
            using (SplitStream splitStream = new(utf8Stream, leaveOpen: true, chunkSize: 4096)) {
                while (await splitStream.MoveNextStreamAsync(requestAborted)) {
                    using (var tracorDataRecord = await System.Text.Json.JsonSerializer.DeserializeAsync<TracorDataRecord>(
                        splitStream, this._JsonSerializerOptions, requestAborted)) {
                        if (tracorDataRecord is { }) {
                            this._TracorCollector.Push(tracorDataRecord);
                        }
                    }
                }
            }
        }
    }
}
