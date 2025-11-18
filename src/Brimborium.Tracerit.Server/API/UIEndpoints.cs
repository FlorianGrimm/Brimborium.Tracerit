using Brimborium.Tracerit.Service;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using System.Net.Http.Headers;
using System.Text.Json;

namespace Brimborium.Tracerit.API;

// singleton
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
        this._JsonSerializerOptions = JsonSerializerOptionsExtensions.AddTracorDataMinimalJsonConverter(null, tracorDataRecordPool);
    }
    public void MapEndpoints(WebApplication app) {
        var group = app.MapGroup("api");
        group.Map("ping", () => {
            return "pong";
        }).AllowAnonymous();

        group.MapGet("DirectoryList", () => {
            return this._LogFileService.DirectoryBrowse();
        }).AllowAnonymous();

        group.MapGet("Current", () => {
            var result = this._TracorCollector.GetListTracorDataRecord();
            var outStream = this._MemoryPoolManager.RecyclableMemoryStreamManager.GetStream();
            byte[] newline = "\r\n"u8.ToArray();
            foreach (var tracorDataRecord in result) {
                System.Text.Json.JsonSerializer.Serialize(outStream, tracorDataRecord, this._JsonSerializerOptions);
                outStream.Write(newline);
            }
            return Results.Stream(outStream, "application/jsonl");
        });

        group.MapGet("File/{name}", (string name) => {
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
}
