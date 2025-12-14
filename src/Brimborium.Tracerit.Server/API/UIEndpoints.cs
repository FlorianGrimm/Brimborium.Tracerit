namespace Brimborium.Tracerit.API;

/// <summary>
/// /api/Tracerit
/// </summary>
/// <remarks>
/// singleton
/// </remarks>
public class UIEndpoints : IController {
    private readonly ITracorServerCollectorReadAndWrite _TracorCollector;
    private readonly TracorMemoryPoolManager _MemoryPoolManager;
    private readonly IReadLogFileService _LogFileService;
    private readonly JsonSerializerOptions _JsonSerializerOptions;

    public UIEndpoints(
        ITracorServerCollectorReadAndWrite tracorCollector,
        TracorMemoryPoolManager memoryPoolManager,
        TracorDataRecordPool tracorDataRecordPool,
        IReadLogFileService logFileService) {
        this._TracorCollector = tracorCollector;
        this._MemoryPoolManager = memoryPoolManager;
        this._LogFileService = logFileService;
        this._JsonSerializerOptions = TracorDataSerialization.AddTracorDataMinimalJsonConverter(null, tracorDataRecordPool);
    }

    public void MapEndpoints(WebApplication app) {
        var group = app.MapGroup("/api/Tracerit");
        group.Map("/ping", () => {
            return "pong";
        }).AllowAnonymous();

        group.MapGet("/CurrentStream/{name?}", (string? name) => {
            return GetCurrentStream(name);
        }).AllowAnonymous()
        .RequireHost(["localhost"])
        ;

        group.MapGet("/DirectoryList", () => {
            return this._LogFileService.DirectoryBrowse();
        }).AllowAnonymous()
        .RequireHost(["localhost"])
        ;

        group.MapGet("/File/{name}", async (HttpContext httpContext, string name) => {
            var nameNormalize = name.TrimStart('\\', '/');
            var result = this._LogFileService.FileContentRead(name);
            if (result is ResponseSuccessful<FileContentReadResponse> { Result: { } responseResult }) {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.ContentType = responseResult.ContentType;
                httpContext.Response.Headers.ContentLength = responseResult.ContentLength;
                if (responseResult.ContentEncoding is { Length: > 0 } contentEncoding) {
                    httpContext.Response.Headers.ContentEncoding = contentEncoding;
                }
                await httpContext.Response.SendFileAsync(responseResult.FileFQ, 0, responseResult.ContentLength, httpContext.RequestAborted);
            } else if (result is IResponseFailed responseFailed) {
                await Results.BadRequest(responseFailed.Error).ExecuteAsync(httpContext);

            } else {
                await Results.InternalServerError().ExecuteAsync(httpContext);
            }
        }).AllowAnonymous()
        .RequireHost(["localhost"]);
    }

    public IResult GetCurrentStream(string? name) {
        ITracorServerCollectorReadAndWrite tracorCollector = this._TracorCollector;
        TracorMemoryPoolManager memoryPoolManager = this._MemoryPoolManager;
        JsonSerializerOptions jsonSerializerOptions = this._JsonSerializerOptions;

        using (var listTracorDataRecord = tracorCollector.GetListTracorDataRecord(name)) {
            var outStream = memoryPoolManager.RecyclableMemoryStreamManager.GetStream();
            byte[] newline = "\r\n"u8.ToArray();
            foreach (var tracorDataRecord in listTracorDataRecord.ListData) {
                System.Text.Json.JsonSerializer.Serialize(outStream, tracorDataRecord, jsonSerializerOptions);
                outStream.Write(newline);
            }
            outStream.Position = 0;
            var resultStream = Results.Stream(outStream, "application/jsonl");
            return resultStream;
        }
    }
}
