using Microsoft.Extensions.Options;

namespace Brimborium.Tracerit.Collector.Services;

public class LogFileService : IDisposable {
    private class ConfigState {
        public string? LogDirectory { get; internal set; }
    }
    private ConfigState _ConfigState = new();
    private IDisposable? _UnwireAppConfig;

    public LogFileService(
        IOptionsMonitor<AppConfig> appConfig
        ) {
        this._UnwireAppConfig = appConfig.OnChange(OnChangeAppConfig);
        this.OnChangeAppConfig(appConfig.CurrentValue);
    }

    private void OnChangeAppConfig(AppConfig options) {
        ConfigState configState = new();
        configState.LogDirectory = options.LogDirectory;

        this._ConfigState = configState;
    }

    public void Dispose() {
        using (var unwireAppConfig = this._UnwireAppConfig) {
            this._UnwireAppConfig = null;
        }
    }

    public ResponseWrapper<DirectoryBrowseResponse> DirectoryBrowse() {
        var configState = this._ConfigState;
        var logDirectory = configState.LogDirectory;
        if (string.IsNullOrEmpty(logDirectory)) {
            return new ResponseFailed<DirectoryBrowseResponse>() { Error = "Configuration" };
        }
        System.IO.DirectoryInfo diLogDirectory = new(logDirectory);
        if (!diLogDirectory.Exists) {
            return new ResponseFailed<DirectoryBrowseResponse>() { Error = "logDirectory: Not Exists" };
        }
        var listFiles = diLogDirectory.EnumerateFiles("*.jsonl", new EnumerationOptions() {
            RecurseSubdirectories = false,
            IgnoreInaccessible = true
        });
        var result = listFiles
            .Select(
                static diLogFile => {
                    return new LogFileInformation(
                        diLogFile.Name,
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