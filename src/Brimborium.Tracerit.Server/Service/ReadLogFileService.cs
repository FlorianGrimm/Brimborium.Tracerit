namespace Brimborium.Tracerit.Service;

public interface IReadLogFileService {
    ResponseWrapper<DirectoryBrowseResponse> DirectoryBrowse();
    ResponseWrapper<FileContentReadResponse> FileContentRead(string name);
}

public sealed class ReadLogFileService : IReadLogFileService, IDisposable {
    private class ConfigState {
        public string? LogDirectory { get; internal set; }
    }
    private ConfigState _ConfigState = new();
    private IDisposable? _UnwireAppConfig;

    public ReadLogFileService(
        IOptionsMonitor<TracorLogFileServiceOptions> options
        ) {
        this._UnwireAppConfig = options.OnChange(this.OnChangeAppConfig);
        this.OnChangeAppConfig(options.CurrentValue);
    }

    private void OnChangeAppConfig(TracorLogFileServiceOptions options) {
        ConfigState configState = new() {
            LogDirectory = PathTrimEnd(options.LogDirectory ?? string.Empty)
        };

        this._ConfigState = configState;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
    public void Dispose() {
        using (var unwireAppConfig = this._UnwireAppConfig) {
            this._UnwireAppConfig = null;
        }
    }

    public ResponseWrapper<DirectoryBrowseResponse> DirectoryBrowse() {
        var configState = this._ConfigState;
        var logDirectory = configState.LogDirectory;
        if (string.IsNullOrEmpty(logDirectory)) {
            return new ResponseFailed<DirectoryBrowseResponse>() { Error = "Configuration: LogDirectory is empty." };
        }
        System.IO.DirectoryInfo diLogDirectory = new(logDirectory);
        if (!diLogDirectory.Exists) {
            return new ResponseFailed<DirectoryBrowseResponse>() { Error = "LogDirectory: Not Exists" };
        }
        var listFilesJsonl = diLogDirectory.EnumerateFiles("*.jsonl", new EnumerationOptions() {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true
        });
        var diLogDirectoryFullName = PathTrimEnd(diLogDirectory.FullName);
        var length = 1 + diLogDirectoryFullName.Length;
        var result = listFilesJsonl
            .Where(
                (diLogFile) => diLogFile.FullName.StartsWith(diLogDirectoryFullName, StringComparison.OrdinalIgnoreCase)
            )
            .Select(
                (diLogFile) => {
                    return new LogFileInformation(
                        diLogFile.FullName.Substring(length).Replace(System.IO.Path.PathSeparator, '/'),
                        diLogFile.CreationTimeUtc,
                        diLogFile.Length);
                })
            .ToList();
        return new ResponseSuccessful<DirectoryBrowseResponse>() {
            Result = new DirectoryBrowseResponse() {
                Files = result
            }
        };
    }

    private static char[] _ArraySlash = new char[] { '/', '\\' };
    private static string PathTrimEnd(string value) {
        return value.TrimEnd(_ArraySlash);
    }

    private static int _SlashPathMode = 0;
    private static string SlashPath(string value) {
        if (_SlashPathMode == 0) {
            _SlashPathMode = ('\\' == System.IO.Path.PathSeparator) ? 1 : 2;
        }
        if (_SlashPathMode == 1) {
            return value.Replace('\\', '/');
        } else {
            return value;
        }
    }

    public ResponseWrapper<FileContentReadResponse> FileContentRead(string name) {
        var configState = this._ConfigState;
        var logDirectory = configState.LogDirectory;
        if (string.IsNullOrEmpty(logDirectory)) {
            return new ResponseFailed<FileContentReadResponse>() { Error = "Configuration" };
        }
        if (name.Contains("..") || System.IO.Path.IsPathFullyQualified(name)) {
            return new ResponseFailed<FileContentReadResponse>() { Error = "logDirectory: No" };
        }
        System.IO.DirectoryInfo diLogDirectory = new(logDirectory);
        if (!diLogDirectory.Exists) {
            return new ResponseFailed<FileContentReadResponse>() { Error = "logDirectory: Not Exists." };
        }
        var listFiles = diLogDirectory.EnumerateFiles(name, new EnumerationOptions() {
            RecurseSubdirectories = false,
            IgnoreInaccessible = true
        });
        var listLogFileInformation = listFiles.Select(static diLogFile => {
            return (
                fullName: diLogFile.FullName,
                lastModified: diLogFile.LastWriteTimeUtc,
                logFileInformation: new LogFileInformation(
                    diLogFile.Name,
                    diLogFile.CreationTimeUtc,
                    diLogFile.Length)
            );
        }).ToList();

        if (1 != listLogFileInformation.Count) {
            return new ResponseFailed<FileContentReadResponse>() {
                Error = "Not found"
            };
        }

        {
            var (fullName, lastModified, logFileInformation) = listLogFileInformation[0];
            //var fileStream = System.IO.File.Open(fullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return new ResponseSuccessful<FileContentReadResponse>() {
                Result = new FileContentReadResponse(
                    LogFileInformation: logFileInformation,
                    FileFQ: fullName,
                    ContentType: "application/jsonl",
                    EnableRangeProcessing: true,
                    LastModified: lastModified
                    )
            };
        }
    }
}