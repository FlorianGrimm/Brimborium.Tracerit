namespace Brimborium.Tracerit.API;

/// <summary>
/// /api/Tracerit
/// </summary>
/// <remarks>
/// singleton
/// </remarks>
public class UIEndpoints : IController {
    private readonly ITracorCollector _TracorCollector;
    private readonly TracorMemoryPoolManager _MemoryPoolManager;
    private readonly LogFileService _LogFileService;
    private readonly JsonSerializerOptions _JsonSerializerOptions;

    public UIEndpoints(
        ITracorCollector tracorCollector,
        TracorMemoryPoolManager memoryPoolManager,
        TracorDataRecordPool tracorDataRecordPool,
        LogFileService logFileService) {
        this._TracorCollector = tracorCollector;
        this._MemoryPoolManager = memoryPoolManager;
        this._LogFileService = logFileService;
        this._JsonSerializerOptions = TracorDataSerialization.GetMinimalJsonSerializerOptions(null, tracorDataRecordPool);
    }

    public void MapEndpoints(WebApplication app) {
        var group = app.MapGroup("/api/Tracerit");
        group.Map("/ping", () => {
            return "pong";
        }).AllowAnonymous();

        group.MapGet("/CurrentStream/{name?}", (string? name) => {
            return GetCurrentStream(name);
        }).AllowAnonymous();

        group.MapGet("/DirectoryList", () => {
            return this._LogFileService.DirectoryBrowse();
        }).AllowAnonymous();

        group.MapGet("/File/{name}", (string name) => {
            var result = this._LogFileService.FileContentRead(name);
            if (result is ResponseSuccessful<FileContentReadResponse> { Result: { } responseResult }) {
                return TypedResults.PhysicalFile(
                    path: responseResult.FileFQ,
                    contentType: responseResult.ContentType,
                    fileDownloadName: responseResult.FileDownloadName,
                    lastModified: responseResult.LastModified,
                    entityTag: responseResult.EntityTag,
                    enableRangeProcessing: responseResult.EnableRangeProcessing);

            } else if (result is IResponseFailed responseFailed) {
                return Results.BadRequest(responseFailed.Error);

            } else {
                return Results.InternalServerError();
            }
        }).AllowAnonymous();
    }

    public IResult GetCurrentStream(string? name) {
        ITracorCollector tracorCollector = this._TracorCollector;
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
