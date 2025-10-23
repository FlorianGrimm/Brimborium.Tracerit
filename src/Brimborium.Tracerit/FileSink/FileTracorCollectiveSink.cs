using System.IO.Compression;

namespace Brimborium.Tracerit.FileSink;

public sealed class FileTracorCollectiveSink : ITracorCollectiveSink, IDisposable {
    private IDisposable? _TracorOptionsMonitorDisposing;
    private IDisposable? _FileTracorOptionsMonitorDisposing;
    private readonly IServiceProvider? _ServiceProvider;
    private CancellationTokenRegistration? _OnApplicationStoppingDisposing;
    private Func<CancellationToken?> _GetOnApplicationStoppingDisposing = () => null;

    private readonly Lock _LockProperties = new Lock();
    private string? _Directory;
    private DateTime _DirectoryRecheck = new DateTime(0);
    private string? _FileName;
    private TimeSpan _Period = TimeSpan.Zero;
    private TimeSpan _FlushPeriod = TimeSpan.Zero;
    private FileTracorCollectiveCompression _Compression = FileTracorCollectiveCompression.None;
    private TracorDataRecord? _Resource;
    private System.IO.Stream? _CurrentFileStream;
    private string? _CurrentFileFQN;
    private long _PeriodStarted;

    private readonly System.Threading.Channels.Channel<ITracorData> _Channel;
    private readonly ChannelWriter<ITracorData> _ChannelWriter;
    private Task? _TaskLoop;
    private readonly CancellationTokenSource _TaskLoopEnds = new();
    private readonly SemaphoreSlim _AsyncLockWriteFile = new(initialCount: 1, maxCount: 1);
    private readonly TracorEmergencyLogging _TracorEmergencyLogging;
    private string? _ApplicationName;
    private bool _DirectoryExists;
    private bool _CleanupEnabled;
    private TimeSpan _CleanupPeriod = TimeSpan.FromDays(31);
    private Task _CleanupTask = Task.CompletedTask;

    public FileTracorCollectiveSink(
        TracorOptions tracorOptions,
        FileTracorOptions fileTracorOptions
        ) : this(tracorOptions, fileTracorOptions, new()) {
    }

    public FileTracorCollectiveSink(
        TracorOptions tracorOptions,
        FileTracorOptions fileTracorOptions,
        TracorEmergencyLogging tracorEmergencyLogging) {
        this._Channel = System.Threading.Channels.Channel.CreateBounded<ITracorData>(10000);
        this._ChannelWriter = this._Channel.Writer;
        this._TracorEmergencyLogging = tracorEmergencyLogging;
        this.SetTracorOptions(tracorOptions);
        this.SetFileTracorOptions(fileTracorOptions);
    }

    public FileTracorCollectiveSink(
        IServiceProvider serviceProvider,
        IOptionsMonitor<TracorOptions> tracorOptions,
        IOptionsMonitor<FileTracorOptions> fileTracorOptions,
        TracorEmergencyLogging tracorEmergencyLogging) {
        this._ServiceProvider = serviceProvider;
        this._Channel = System.Threading.Channels.Channel.CreateBounded<ITracorData>(10000);
        this._ChannelWriter = this._Channel.Writer;
        this._TracorEmergencyLogging = tracorEmergencyLogging;

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
                && options.OnGetApplicationStopping is { } getApplicationStopping) {
                // using the callback avoids 
                this._GetOnApplicationStoppingDisposing = (() => getApplicationStopping(serviceProvider));
            }
            this._CleanupEnabled = options.CleanupEnabled;
            this._CleanupPeriod = options.CleanupPeriod;

            // reset PeriodStarted since _Period may have change
            this._PeriodStarted = 0;

            string? directory = GetDirectory(
                options.BaseDirectory,
                options.GetBaseDirectory,
                options.Directory);
            if (string.IsNullOrWhiteSpace(directory)) {
                this._Directory = null;
                this._FileName = null;
                this._Period = TimeSpan.Zero;
                this._FlushPeriod = TimeSpan.Zero;
                this._Compression = FileTracorCollectiveCompression.None;
                if (this._TracorEmergencyLogging.IsEnabled) {
                    this._TracorEmergencyLogging.Log("FileTracorCollectiveSink directory is empty");
                }
            } else {
                this._Directory = directory;
                this._FileName = options.FileName;
                this._Period = options.Period;
                this._FlushPeriod = options.FlushPeriod;
                this._Compression = options.Compression switch {
                    "brotli" => FileTracorCollectiveCompression.Brotli,
                    "gzip" => FileTracorCollectiveCompression.Gzip,
                    _ => FileTracorCollectiveCompression.None
                };
                if (options.GetResource() is { } resource) {
                    this._Resource = new TracorDataRecord() {
                        TracorIdentifier = new("Resource", resource.TracorIdentifier.Scope),
                        Timestamp = DateTime.UtcNow
                    };
                    this._Resource.ListProperty.AddRange(resource.ListProperty);
                } else {
                    this._Resource = new TracorDataRecord() {
                        TracorIdentifier = new("Resource", this._ApplicationName ?? string.Empty),
                        Timestamp = DateTime.UtcNow
                    };
                }
                if (this._TracorEmergencyLogging.IsEnabled) {
                    this._TracorEmergencyLogging.Log($"FileTracorCollectiveSink directory:{directory}");
                }
            }
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
        var timestamp = utcNow.ToString("yyyy-MM-dd-HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        if (this._FileName is { Length: > 0 } fileName) {
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

    public string? GetCurrentFileFQN() => this._CurrentFileFQN;

    public DateTimeOffset PeriodStarted() => new DateTimeOffset(this._PeriodStarted * this._Period.Ticks, TimeSpan.Zero);

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
                if (this._TracorEmergencyLogging.IsEnabled) {
                    this._TracorEmergencyLogging.Log("FileTracorCollectiveSink no entries to write.");
                }
                return false;
            }

            {
                await this.WriteAsync(listTracorData, listTracorDataProperty);

                return true;
            }
        } finally {
            this._AsyncLockWriteFile.Release();
        }
    }

    private async Task WriteAsync(List<ITracorData> listTracorData, List<TracorDataProperty> listTracorDataProperty) {
        DateTime utcNow = DateTime.UtcNow;
        TimeSpan statePeriod;
        long statePeriodStarted;
        Stream? currentFileStream;
        long currentPeriodStarted;
        using (this._LockProperties.EnterScope()) {
            statePeriod = this._Period;
            statePeriodStarted = this._PeriodStarted;
            currentFileStream = this._CurrentFileStream;
            currentPeriodStarted = utcNow.Ticks / statePeriod.Ticks;
        }

        {
            if (currentFileStream is not null) {
                if (1 <= statePeriod.TotalSeconds) {
                    if (statePeriodStarted != currentPeriodStarted) {
                        currentFileStream.Close();
                        currentFileStream = null;
                        if (this._CurrentFileFQN is { Length: > 0 } currentFileFQN) {
                            if (this._Compression != FileTracorCollectiveCompression.None) {
                                if (this._CleanupTask.IsCompleted) {
                                    this._CleanupTask = CompressFileAsync(currentFileFQN, this._Compression);
                                } else {
                                    this._CleanupTask = this._CleanupTask.ContinueWith(async (_) => {
                                        await CompressFileAsync(currentFileFQN, this._Compression);
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        bool addNewLine = false;
        bool addResource = false;

        if (currentFileStream is null && this._Directory is { Length: > 0 } directory) {
            if (utcNow < this._DirectoryRecheck) {
                // no need to check
            } else {
                this._DirectoryRecheck = utcNow.AddHours(1);
                if (System.IO.Directory.Exists(directory)) {
                    this._DirectoryExists = true;
                    if (this._CleanupEnabled) {
                        if (12 < this._CleanupPeriod.TotalHours) {
                            var limit = utcNow.Subtract(this._CleanupPeriod);
                            if (_CleanupTask.IsCompleted) {
                                this._CleanupTask = this.DeleteOldLogFilesAsync(limit, directory);
                            }
                        }
                    }
                } else {
                    try {
                        System.IO.Directory.CreateDirectory(directory);
                        this._DirectoryExists = true;
                        if (this._TracorEmergencyLogging.IsEnabled) {
                            this._TracorEmergencyLogging.Log($"FileTracorCollectiveSink created directory:{directory}");
                        }
                    } catch (Exception error) {
                        if (this._TracorEmergencyLogging.IsEnabled) {
                            this._TracorEmergencyLogging.Log($"FileTracorCollectiveSink cannot create directory:{directory} {error.Message}");
                        }
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
                    this._CurrentFileFQN = logFileFQN;
                    addResource = created;
                    addNewLine = !created;

                    this._PeriodStarted = currentPeriodStarted;

                    if (this._TracorEmergencyLogging.IsEnabled) {
                        this._TracorEmergencyLogging.Log($"FileTracorCollectiveSink new file:{logFileFQN}");
                    }
                }
            }
        }

        if (currentFileStream is { }) {
            byte[] nl = (_ArrayNewLine ??= Encoding.UTF8.GetBytes(System.Environment.NewLine));
            System.Text.Json.JsonSerializerOptions jsonSerializerOptions =
                TracorDataSerialization.GetMinimalJsonSerializerOptions(null, null);

            if (addNewLine) {
                currentFileStream.Write(nl, 0, nl.Length);
            }
            if (addResource) {
                if (this._Resource is { } resource) {
                    System.Text.Json.JsonSerializer.Serialize(currentFileStream, resource, jsonSerializerOptions);
                    currentFileStream.Write(nl, 0, nl.Length);
                }
            }

            foreach (var tracorData in listTracorData) {
                listTracorDataProperty.Clear();
                tracorData.ConvertProperties(listTracorDataProperty);

                System.Text.Json.JsonSerializer.Serialize(currentFileStream, tracorData, jsonSerializerOptions);
                currentFileStream.Write(nl, 0, nl.Length);

                if (tracorData is IReferenceCountObject referenceCountObject) {
                    referenceCountObject.Dispose();
                }
            }
            listTracorDataProperty.Clear();
            listTracorData.Clear();
            await currentFileStream.FlushAsync();

            if (this._TracorEmergencyLogging.IsEnabled) {
                this._TracorEmergencyLogging.Log($"FileTracorCollectiveSink entries flushed.");
            }
        }
    }

    private (FileStream stream, bool created) GetLogFileStream(string logFilePath) {
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
                } else {
                    if (this._Compression is FileTracorCollectiveCompression.Brotli or FileTracorCollectiveCompression.Gzip) {
                        await CompressFileAsync(fileInfo.FullName, this._Compression);
                    }
                }
            }
        } catch (Exception error) {
            System.Console.Error.WriteLine(error.ToString());
        }
    }

    public const string FileExtensionJsonl = ".jsonl";
    public const string FileExtensionJsonlBrotli = ".jsonl.brotli";
    public const string FileExtensionJsonlGzip = ".jsonl.gzip";
    public const string FileExtensionBrotli = ".brotli";
    public const string FileExtensionGzip = ".gzip";

    public static FileTracorCollectiveCompression? GetCompressionFromFileName(string currentFileFQN) {
        if (currentFileFQN.EndsWith(FileExtensionJsonl)) {
            if (currentFileFQN.EndsWith(FileExtensionJsonlBrotli)) { return FileTracorCollectiveCompression.Brotli; }
            if (currentFileFQN.EndsWith(FileExtensionJsonlGzip)) { return FileTracorCollectiveCompression.Gzip; }
            return FileTracorCollectiveCompression.None;
        } else {
            return null;
        }
    }

    public static async Task CompressFileAsync(string currentFileFQN, FileTracorCollectiveCompression compression) {
        var c = GetCompressionFromFileName(currentFileFQN);
        if (c is FileTracorCollectiveCompression.None) {
            // compress
        } else {
            return;
        }

        if (FileTracorCollectiveCompression.Brotli == compression) {
            var compressedFileFQN = currentFileFQN + FileExtensionBrotli;
            using (var readFileStream = System.IO.File.Open(currentFileFQN, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.ReadWrite)) {
                using (var writeFileStream = System.IO.File.OpenWrite(compressedFileFQN)) {
                    using (var brotliStream = new BrotliStream(writeFileStream,
                        compressionOptions: new BrotliCompressionOptions() {
                            Quality = 6
                        }, leaveOpen: true)) {
                        await readFileStream.CopyToAsync(brotliStream);
                        await brotliStream.FlushAsync();
                        await writeFileStream.FlushAsync();
                    }
                }
            }
            System.IO.File.Delete(currentFileFQN);
            return;
        }

        if (FileTracorCollectiveCompression.Gzip == compression) {
            var compressedFileFQN = currentFileFQN + FileExtensionGzip;
            using (var readFileStream = System.IO.File.Open(currentFileFQN, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.ReadWrite)) {
                using (var writeFileStream = System.IO.File.OpenWrite(compressedFileFQN)) {
                    using (var gzipStream = new GZipStream(writeFileStream,
                        compressionOptions: new ZLibCompressionOptions() {
                            CompressionLevel = 9,
                            CompressionStrategy = ZLibCompressionStrategy.Default
                        }, leaveOpen: true)) {
                        await readFileStream.CopyToAsync(gzipStream);
                        await gzipStream.FlushAsync();
                        await writeFileStream.FlushAsync();
                    }
                }
            }
            System.IO.File.Delete(currentFileFQN);
            return;
        }
    }
}