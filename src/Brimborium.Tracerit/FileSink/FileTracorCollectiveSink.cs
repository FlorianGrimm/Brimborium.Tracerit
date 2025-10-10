namespace Brimborium.Tracerit.FileSink;

public sealed class FileTracorCollectiveSink : ITracorCollectiveSink, IDisposable {
    private IDisposable? _TracorOptionsMonitorDisposing;
    private IDisposable? _FileTracorOptionsMonitorDisposing;
    private readonly IServiceProvider? _ServiceProvider;
    private CancellationTokenRegistration? _OnApplicationStoppingDisposing;
    private Func<CancellationToken?> _GetOnApplicationStoppingDisposing = () => null;

    private Lock _LockProperties = new Lock();
    private string? _Directory;
    private DateTime _DirectoryRecheck = new DateTime(0);
    private string? _FileName;
    private TimeSpan _Period = TimeSpan.Zero;
    private TimeSpan _FlushPeriod = TimeSpan.Zero;

    private System.IO.Stream? _CurrentFileStream;
    private long _PeriodStarted;

    private readonly System.Threading.Channels.Channel<ITracorData> _Channel;
    private readonly ChannelWriter<ITracorData> _ChannelWriter;
    private Task? _TaskLoop;
    private readonly CancellationTokenSource _TaskLoopEnds = new();
    private readonly SemaphoreSlim _AsyncLockWriteFile = new(initialCount: 1, maxCount: 1);
    private string? _ApplicationName;
    private bool _DirectoryExists;
    private bool _CleanupEnabled;
    private TimeSpan _CleanupPeriod = TimeSpan.FromDays(31);

    public FileTracorCollectiveSink(
        TracorOptions tracorOptions,
        FileTracorOptions fileTracorOptions) {
        this._Channel = System.Threading.Channels.Channel.CreateBounded<ITracorData>(10000);
        this._ChannelWriter = this._Channel.Writer;
        this.SetTracorOptions(tracorOptions);
        this.SetFileTracorOptions(fileTracorOptions);
    }

    public FileTracorCollectiveSink(
        IServiceProvider serviceProvider,
        IOptionsMonitor<TracorOptions> tracorOptions,
        IOptionsMonitor<FileTracorOptions> fileTracorOptions) {
        this._ServiceProvider = serviceProvider;
        this._Channel = System.Threading.Channels.Channel.CreateBounded<ITracorData>(10000);
        this._ChannelWriter = this._Channel.Writer;

        this._TracorOptionsMonitorDisposing = tracorOptions.OnChange(this.SetTracorOptions);
        this._FileTracorOptionsMonitorDisposing = fileTracorOptions.OnChange(this.SetFileTracorOptions);
        this.SetTracorOptions(tracorOptions.CurrentValue);
        this.SetFileTracorOptions(fileTracorOptions.CurrentValue);
    }

    private void SetTracorOptions(TracorOptions tracorOptions) {
        if (tracorOptions.ApplicationName is { Length: > 0 } applicationName) {
            this._ApplicationName = applicationName;
        } else if (this._ApplicationName is null) {
            this._ApplicationName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
        }
    }

    internal void SetFileTracorOptions(FileTracorOptions options) {
        using (this._LockProperties.EnterScope()) {
            if (this._ServiceProvider is { } serviceProvider
                && options.GetApplicationStopping is { } getApplicationStopping) {
                /*
                if (this._OnApplicationStoppingDisposing is { } old) {
                    old.Unregister();
                }
                var applicationStopping = getApplicationStopping(serviceProvider);
                this._OnApplicationStoppingDisposing = applicationStopping.Register(this.OnApplicationStopping);
                */
                this._GetOnApplicationStoppingDisposing = (() => getApplicationStopping(serviceProvider));
            }
            string? directory = GetDirectory(
                options.BaseDirectory,
                options.GetBaseDirectory,
                options.Directory);
            if (string.IsNullOrWhiteSpace(directory)) {
                this._Directory = null;
                this._FileName = null;
                this._Period = TimeSpan.Zero;
                this._FlushPeriod = TimeSpan.Zero;
            } else {
                this._Directory = directory;
                this._FileName = options.FileName;
                this._Period = options.Period;
                this._FlushPeriod = options.FlushPeriod;
            }
            this._CleanupEnabled = options.CleanupEnabled;
            this._CleanupPeriod = options.CleanupPeriod;
        }
    }

    public static string? GetDirectory(
        string? baseDirectory,
        Func<string?>? getBaseDirectory,
        string? directory) {
        var directoryNormalized = (directory is { Length: > 0 })
            ? directory.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar)
            : null;

        /// if directory is FullyQualified use this
        if (directoryNormalized is { Length: 0 } directoryNormalizedResult
            && System.IO.Path.IsPathFullyQualified(directoryNormalizedResult)) {
            return directoryNormalizedResult;
        }

        // baseDirectory or getBaseDirectory or System.AppContext.BaseDirectory
        string? baseDirectoryNormalized;
        {
            if (baseDirectory is { Length: > 0 }) {
                baseDirectoryNormalized = baseDirectory.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
                if (System.IO.Directory.Exists(baseDirectoryNormalized)) {
                    // ok
                } else {
                    return null;
                }
            } else if (getBaseDirectory is { }
                && getBaseDirectory() is { Length: > 0 } gottenBaseDirectory) {
                baseDirectoryNormalized = gottenBaseDirectory.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
                if (System.IO.Directory.Exists(baseDirectoryNormalized)) {
                    // ok
                } else {
                    return null;
                }
            } else {
                baseDirectoryNormalized = System.AppContext.BaseDirectory;
            }
        }

        var directoryCombined = (directoryNormalized is { Length: > 0 })
            ? System.IO.Path.Combine(baseDirectoryNormalized, directoryNormalized)
            : baseDirectoryNormalized;

        return directoryCombined;
    }

    public string GetLogFilePath(DateTime utcNow) {
        var timestamp = utcNow.ToString("yyyy-MM-dd-HH-mm", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        if (this._FileName is { Length: > 0 } fileName) {
            // "{ApplicationName}_{TimeStamp}.jsonl";
            return fileName
                .Replace("{ApplicationName}", this._ApplicationName)
                .Replace("{TimeStamp}", timestamp)
                ;
        } else {
            return @$"log-{this._ApplicationName}-{timestamp}.jsonl";
        }
    }

    public string GetLogFilePattern() {
        if (this._FileName is { Length: > 0 } fileName) {
            return fileName
                .Replace("{ApplicationName}", this._ApplicationName)
                .Replace("{TimeStamp}", "*")
                ;
        } else {
            return @$"log-{this._ApplicationName}-*.jsonl";
        }
    }

    private void OnApplicationStopping() {
        this._ChannelWriter.Complete();
        this._FlushPeriod = TimeSpan.Zero;
        var _ = this.FlushAsync().ContinueWith((t) => { this.Dispose(); });
    }

    private void Dispose(bool disposing) {
        using (var optionsMonitorDisposing = this._FileTracorOptionsMonitorDisposing) {
            using (var onApplicationStoppingDisposing = this._OnApplicationStoppingDisposing) {
                if (disposing) {
                    this._FileTracorOptionsMonitorDisposing = null;
                    this._OnApplicationStoppingDisposing = null;
                }
                this.Flush();
            }
        }
    }

    ~FileTracorCollectiveSink() {
        this.Dispose(disposing: false);
    }

    public void Dispose() {
        this.Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
        
    public bool IsGeneralEnabled() => true;

    public bool IsEnabled() => true;

    private static byte[]? _ArrayNewLine;

    public void OnTrace(bool isPublic, ITracorData tracorData) {
        if (tracorData is IReferenceCountObject referenceCountObject) {
            referenceCountObject.IncrementReferenceCount();
        }
        if (this._ChannelWriter.TryWrite(tracorData)) {
            // OK
        } else {
            // channel full
        }
        if (this._TaskLoop is null) {
            this._TaskLoop = Task.CompletedTask;
            this._TaskLoop = Task.Run(this.Loop).ContinueWith((_) => { this._TaskLoop = null; });
        }
    }

    private async Task Loop() {
        using (this._LockProperties.EnterScope()) {
            if (this._GetOnApplicationStoppingDisposing() is { } applicationStopping) {
                this._OnApplicationStoppingDisposing = applicationStopping.Register(this.OnApplicationStopping);
            }
        }

        var reader = this._Channel.Reader;
        List<ITracorData> listTracorData = new(1000);
        List<TracorDataProperty> listTracorDataProperty = new(1000);
        try {
            int watchDog = 0;
            while (!this._TaskLoopEnds.IsCancellationRequested) {
                if (watchDog < 10) {
                    await Task.Delay(this._FlushPeriod);
                    watchDog++;
                } else {
                    await reader.WaitToReadAsync(this._TaskLoopEnds.Token);
                }
                if (await this.FlushAsync(reader, listTracorData, listTracorDataProperty)) {
                    watchDog = 0;
                }
            }
            await this.FlushAsync(reader, listTracorData, listTracorDataProperty);
        } catch (System.Exception error) {
            System.Console.Error.WriteLine(error.ToString());
        }
    }

    private async Task<bool> FlushAsync(
        ChannelReader<ITracorData> reader,
        List<ITracorData> listTracorData,
        List<TracorDataProperty> listTracorDataProperty) {
        await this._AsyncLockWriteFile.WaitAsync();
        try {
            while (reader.TryRead(out var tracorData)) {
                listTracorData.Add(tracorData);
            }
            if (0 == listTracorData.Count) {
                return false;
            }

            {
                await this.WriterAsync(listTracorData, listTracorDataProperty);
                listTracorData.Clear();
                return true;
            }
        } finally {
            this._AsyncLockWriteFile.Release();
        }
    }

    private async Task WriterAsync(List<ITracorData> listTracorData, List<TracorDataProperty> listTracorDataProperty) {
        DateTime utcNow = DateTime.UtcNow;
        TimeSpan statePeriod;
        long statePeriodStarted;
        Stream? currentFileStream;
        long currentPeriodStarted;
        bool addNewLine = true;
        using (this._LockProperties.EnterScope()) {
            statePeriod = this._Period;
            statePeriodStarted = this._PeriodStarted;
            currentFileStream = this._CurrentFileStream;
            currentPeriodStarted = utcNow.Ticks / statePeriod.Ticks;
        }

        {
            if (currentFileStream is not null) {
                if (60 <= statePeriod.TotalSeconds) {
                    if (statePeriodStarted != currentPeriodStarted) {
                        currentFileStream.Close();
                        currentFileStream = null;
                    }
                }
            }
        }

        Task taskDeleteOld = Task.CompletedTask;

        if (currentFileStream is null && this._Directory is { Length: > 0 } directory) {
            if (utcNow < this._DirectoryRecheck) {
                // no need to check
            } else {
                this._DirectoryRecheck = utcNow.AddDays(6);
                if (System.IO.Directory.Exists(directory)) {
                    this._DirectoryExists = true;
                    if (this._CleanupEnabled) {
                        if (12 < this._CleanupPeriod.TotalHours) {
                            var limit = utcNow.Subtract(this._CleanupPeriod);
                            taskDeleteOld = this.DeleteOldLogFilesAsync(limit, directory);
                        }
                    }
                } else {
                    try {
                        System.IO.Directory.CreateDirectory(directory);
                        this._DirectoryExists = true;
                    } catch {
                    }
                }
            }
            if (!this._DirectoryExists) {
                return;
            } else {
                using (this._LockProperties.EnterScope()) {
                    var logFilePath = this.GetLogFilePath(new DateTime(currentPeriodStarted * statePeriod.Ticks));
                    var logFileFQN = System.IO.Path.Combine(directory, logFilePath);
                    bool created;
                    (currentFileStream, created) = this.GetLogFileStream(logFileFQN);
                    this._CurrentFileStream = currentFileStream;
                    if (created) { addNewLine = false; }

                    this._PeriodStarted = currentPeriodStarted;
                }
            }
        }

        if (currentFileStream is { }) {
            byte[] nl = (_ArrayNewLine ??= Encoding.UTF8.GetBytes(System.Environment.NewLine));
            System.Text.Json.JsonSerializerOptions jsonSerializerOptions = this.GetJsonSerializerOptions();

            foreach (var tracorData in listTracorData) {

                listTracorDataProperty.Clear();
                tracorData.ConvertProperties(listTracorDataProperty);
                if (addNewLine) {
                    currentFileStream.Write(nl, 0, nl.Length);
                } else {
                    addNewLine = true;
                }

                System.Text.Json.JsonSerializer.Serialize(currentFileStream, tracorData, jsonSerializerOptions);

                if (tracorData is IReferenceCountObject referenceCountObject) {
                    referenceCountObject.Dispose();
                }
            }
            listTracorDataProperty.Clear();
            listTracorData.Clear();
            await currentFileStream.FlushAsync();
        }

        await taskDeleteOld;
    }

    private (Stream? stream, bool created) GetLogFileStream(string logFilePath) {
        if (System.IO.File.Exists(logFilePath)) {
            return (
                stream: new System.IO.FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read),
                created: false);
        } else {
            return (
                stream: new System.IO.FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read),
                created: true);
        }
    }

    private void Flush() {
        this.FlushAsync().GetAwaiter().GetResult();
    }

    public async Task FlushAsync() {
        var reader = this._Channel.Reader;
        List<ITracorData> listTracorData = new(1000);
        List<TracorDataProperty> listTracorDataProperty = new(128);
        await this.FlushAsync(reader, listTracorData, listTracorDataProperty);
    }

    private async Task DeleteOldLogFilesAsync(DateTime limit, string directory) {
        try {
            System.IO.DirectoryInfo di = new(directory);
            var pattern = this.GetLogFilePattern();
            var enumerateFiles = di.EnumerateFiles(pattern, new EnumerationOptions() {
                RecurseSubdirectories = false,
                IgnoreInaccessible = true,
            });
            foreach (var fileInfo in enumerateFiles) {
                if (fileInfo.CreationTimeUtc < limit) {
                    try {
                        fileInfo.Delete();
                    } catch (Exception error) {
                        System.Console.Error.WriteLine(error.ToString());
                    }
                }
            }
        } catch {
        }
    }

    private JsonSerializerOptions? _CacheGetJsonSerializerOptions;

    private JsonSerializerOptions GetJsonSerializerOptions() {
        if (this._CacheGetJsonSerializerOptions is { } result) { return result; }
        {
            result = new();
            result.Converters.Add(new TracorDataJsonMinimalConverterFactory());
            result.Converters.Add(new TracorDataRecordMinimalJsonConverter());
            result.Converters.Add(new TracorDataPropertyMinimalJsonConverter());
            
            /*
            result.Converters.Add(new TracorDataJsonSimpleConverterFactory());
            result.Converters.Add(new TracorDataRecordSimpleJsonConverter());
            result.Converters.Add(new TracorDataPropertySimpleJsonConverter());
            */
            this._CacheGetJsonSerializerOptions = result;
            return result;
        }
    }
}
