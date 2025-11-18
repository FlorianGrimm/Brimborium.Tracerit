namespace Brimborium.Tracerit.API;

public sealed record class LogFileInformation(
    string Name, DateTime CreationTimeUtc, long Length
    );

public sealed class DirectoryBrowseResponse {
    public required List<LogFileInformation> Files { get; set; }
}

public sealed record class FileContentReadResponse(
    LogFileInformation LogFileInformation,
    string FileFQ,    
    string? ContentType = null,
    string? FileDownloadName = null,
    bool EnableRangeProcessing = false,
    DateTimeOffset? LastModified = null,
    Microsoft.Net.Http.Headers.EntityTagHeaderValue? EntityTag = null
    );
