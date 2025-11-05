namespace Brimborium.Tracerit.FileSink;

public sealed class TracorCollectiveFileSink
    : TracorCollectiveBulkSink<TracorFileSinkOptions> {
    //private IDisposable? _TracorOptionsMonitorDisposing;
    //private IDisposable? _FileTracorOptionsMonitorDisposing;
    //private readonly IServiceProvider? _ServiceProvider;
    //private CancellationTokenRegistration? _OnApplicationStoppingDisposing;
    //private Func<CancellationToken?> _GetOnApplicationStoppingDisposing = () => null;

    //private readonly Lock _LockProperties = new Lock();
    private string? _Directory;
    private DateTime _DirectoryRecheck = new DateTime(0);
    private string? _FileName;
    private TimeSpan _Period = TimeSpan.Zero;
    //private TimeSpan _FlushPeriod = TimeSpan.Zero;
    private TracorCompression _Compression = TracorCompression.None;
    //private TracorDataRecord? _Resource;
    private System.IO.Stream? _CurrentFileStream;
    private string? _CurrentFileFQN;
    private long _PeriodStarted;

    //private readonly System.Threading.Channels.Channel<ITracorData> _Channel;
    //private readonly ChannelWriter<ITracorData> _ChannelWriter;
    //private Task? _TaskLoop;
    //private readonly CancellationTokenSource _TaskLoopEnds = new();
    //private readonly SemaphoreSlim _AsyncLockWriteFile = new(initialCount: 1, maxCount: 1);
    //private readonly TracorEmergencyLogging _TracorEmergencyLogging;
    //private string? _ApplicationName;
    private bool _DirectoryExists;
    private bool _CleanupEnabled;
    private TimeSpan _CleanupPeriod = TimeSpan.FromDays(31);
    private Task _CleanupTask = Task.CompletedTask;

    public TracorCollectiveFileSink(
        TracorOptions tracorOptions,
        TracorFileSinkOptions fileTracorOptions
        ) : this(tracorOptions, fileTracorOptions, new()) {
    }

    public TracorCollectiveFileSink(
        TracorOptions tracorOptions,
        TracorFileSinkOptions fileTracorOptions,
        TracorEmergencyLogging tracorEmergencyLogging)
        : base(tracorOptions, fileTracorOptions, tracorEmergencyLogging) {
    }

    public TracorCollectiveFileSink(
        IServiceProvider serviceProvider,
        IOptionsMonitor<TracorOptions> tracorOptions,
        IOptionsMonitor<TracorFileSinkOptions> fileTracorOptions,
        TracorEmergencyLogging tracorEmergencyLogging
        ) : base(serviceProvider, tracorOptions, fileTracorOptions, tracorEmergencyLogging) {
    }

    internal override void SetBulkSinkOptionsExtended(TracorFileSinkOptions options) {

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
            this._Compression = TracorCompression.None;
            if (this._TracorEmergencyLogging.IsEnabled) {
                this._TracorEmergencyLogging.Log("FileTracorCollectiveSink directory is empty");
            }
        } else {
            this._Directory = directory;
            this._FileName = options.FileName;
            this._Period = options.Period;
            this._FlushPeriod = options.FlushPeriod;
            this._Compression = options.Compression switch {
                "brotli" => TracorCompression.Brotli,
                "gzip" => TracorCompression.Gzip,
                _ => TracorCompression.None
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
                this._TracorEmergencyLogging.Log($"{this.GetType().Name} directory:{directory}");
            }
        }
    }

    public string? GetCurrentFileFQN() => this._CurrentFileFQN;

    public DateTimeOffset PeriodStarted() => new DateTimeOffset(this._PeriodStarted * this._Period.Ticks, TimeSpan.Zero);


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
        var timestamp = utcNow.ToString("yyyy-MM-dd-HH-mm-ss", TracorConstants.TracorCulture.DateTimeFormat);
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

    protected override async Task WriteAsync(List<ITracorData> listTracorData) {
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
                            if (this._Compression != TracorCompression.None) {
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
        bool addNewLine;
        bool addResource;

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
                            if (this._CleanupTask.IsCompleted) {
                                this._CleanupTask = this.DeleteOldLogFilesAsync(limit, directory);
                            }
                        }
                    }
                } else {
                    try {
                        System.IO.Directory.CreateDirectory(directory);
                        this._DirectoryExists = true;
                        if (this._TracorEmergencyLogging.IsEnabled) {
                            this._TracorEmergencyLogging.Log($"{this.GetType().Name} created directory:{directory}");
                        }
                    } catch (Exception error) {
                        if (this._TracorEmergencyLogging.IsEnabled) {
                            this._TracorEmergencyLogging.Log($"{this.GetType().Name} cannot create directory:{directory} {error.Message}");
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
                        this._TracorEmergencyLogging.Log($"{this.GetType().Name} new file:{logFileFQN}");
                    }
                }
            }
        } else {
            addNewLine = false;
            addResource = false;
        }

        if (currentFileStream is { } currentStream) {
            await this.ConvertAndWriteAsync(
                listTracorData,
                addNewLine,
                addResource, currentStream);

            listTracorData.Clear();
            await currentStream.FlushAsync();

            if (this._TracorEmergencyLogging.IsEnabled) {
                this._TracorEmergencyLogging.Log($"{this.GetType().Name} entries flushed.");
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
                    if (this._Compression is TracorCompression.Brotli or TracorCompression.Gzip) {
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

    public static TracorCompression? GetCompressionFromFileName(string currentFileFQN) {
        if (currentFileFQN.EndsWith(FileExtensionJsonl)) {
            if (currentFileFQN.EndsWith(FileExtensionJsonlBrotli)) { return TracorCompression.Brotli; }
            if (currentFileFQN.EndsWith(FileExtensionJsonlGzip)) { return TracorCompression.Gzip; }
            return TracorCompression.None;
        } else {
            return null;
        }
    }

    public static async Task CompressFileAsync(string currentFileFQN, TracorCompression compression) {
        var c = GetCompressionFromFileName(currentFileFQN);
        if (c is TracorCompression.None) {
            // compress
        } else {
            return;
        }

        if (TracorCompression.Brotli == compression) {
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

        if (TracorCompression.Gzip == compression) {
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
