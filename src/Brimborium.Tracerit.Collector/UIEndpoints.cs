
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Brimborium.Tracerit.Collector;

// singleton
public class UIEndpoints {
    private readonly LogFileService _LogFileService;

    public UIEndpoints(LogFileService logFileService) {
        this._LogFileService = logFileService;
    }
    public void MapUiEndpoints(WebApplication app) {
        var group = app.MapGroup("api");
        group.Map("ping", () => {
            return "pong";
        }).AllowAnonymous();

        group.MapGet("DirectoryList", () => {
            return this._LogFileService.DirectoryBrowse();
        }).AllowAnonymous();


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
