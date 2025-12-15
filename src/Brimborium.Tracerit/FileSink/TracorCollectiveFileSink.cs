namespace Brimborium.Tracerit.FileSink;

public sealed class TracorCollectiveFileSink
    : TracorCollectiveBulkSink<TracorFileSinkOptions> {
    private bool _ConfigurationDirectoryAllowCreation;
    private string? _ConfigurationDirectory;
    private string? _ConfigurationFileName;
    private TimeSpan _ConfigurationPeriod = TimeSpan.FromDays(1);
    private TracorCompression _ConfigurationCompression = TracorCompression.None;
    private bool _ConfigurationCleanupEnabled;
    private TimeSpan _ConfigurationCleanupPeriod = TimeSpan.FromDays(31);

    internal TracorCollectiveFileSink(
        TracorOptions tracorOptions,
        TracorFileSinkOptions fileTracorOptions
        ) : this(tracorOptions, fileTracorOptions, new()) {
    }

    internal TracorCollectiveFileSink(
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
        this._ConfigurationDirectoryAllowCreation = options.DirectoryAllowCreation;
        this._ConfigurationCleanupEnabled = options.CleanupEnabled;
        this._ConfigurationCleanupPeriod = options.CleanupPeriod;

        // reset PeriodStarted since _Period may have change
        foreach (var sinkOfResourceName in this._ByResourceName.Values) {
            sinkOfResourceName.ResetForOptionsChange();
        }

        string? directory = this.GetDirectory(
            options.BaseDirectory,
            options.GetBaseDirectory,
            options.Directory,
            this._ApplicationName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(directory)) {
            this._ConfigurationDirectory = null;
            this._ConfigurationFileName = null;
            this._ConfigurationPeriod = TimeSpan.Zero;
            this._FlushPeriod = TimeSpan.Zero;
            this._ConfigurationCompression = TracorCompression.None;
            if (this._TracorEmergencyLogging.IsEnabled) {
                this._TracorEmergencyLogging.Log("FileTracorCollectiveSink directory is empty");
            }
        } else {
            this._ConfigurationDirectory = directory;
            this._ConfigurationFileName = options.FileName;
            if (options.Period != TimeSpan.Zero) {
                this._ConfigurationPeriod = options.Period;
            }
            this._FlushPeriod = options.FlushPeriod;
            this._ConfigurationCompression = options.Compression?.ToLowerInvariant() switch {
                "brotli" => TracorCompression.Brotli,
                "gzip" => TracorCompression.Gzip,
                _ => TracorCompression.None
            };

            if (this._TracorEmergencyLogging.IsEnabled) {
                this._TracorEmergencyLogging.Log($"{this.GetType().Name} directory:{directory}");
            }
        }
    }

    public string? GetCurrentFileFQN() {
        if (this._ApplicationName is { Length: > 0 } applicationName
            && this._ByResourceName.TryGetValue(applicationName, out var sinkOfResourceName)
            ) {
            return sinkOfResourceName._CurrentFileFQN;
        }
        return default;
    }

    public DateTimeOffset PeriodStarted() {
        if (this._ApplicationName is { Length: > 0 } applicationName
            && this._ByResourceName.TryGetValue(applicationName, out var sinkOfResourceName)
            ) {
            return sinkOfResourceName.PeriodStarted();
        }
        return default;
    }

    public string? GetDirectory(
        string? baseDirectory,
        Func<string?>? getBaseDirectory,
        string? directory,
        string applicationName
        ) {
        var directoryNormalized = (directory is { Length: > 0 })
            ? directory
                .Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar)
                .Replace("{ApplicationName}", applicationName)
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
                baseDirectoryNormalized = HandleReplacements(baseDirectory, applicationName);

                if (!System.IO.Path.IsPathFullyQualified(baseDirectoryNormalized)) {
                    baseDirectoryNormalized = System.IO.Path.GetFullPath(
                            System.IO.Path.Combine(
                                System.AppContext.BaseDirectory,
                                baseDirectoryNormalized.TrimStart(System.IO.Path.DirectorySeparatorChar
                            )
                        )
                    );
                }
                if (System.IO.Directory.Exists(baseDirectoryNormalized)) {
                    // ok
                } else {
                    if (this._ConfigurationDirectoryAllowCreation) {
                        System.IO.Directory.CreateDirectory(baseDirectoryNormalized);
                        // ok
                    } else {
                        return null;
                    }
                }
            } else if (getBaseDirectory is { }
                && getBaseDirectory() is { Length: > 0 } gottenBaseDirectory) {
                baseDirectoryNormalized = HandleReplacements(gottenBaseDirectory, applicationName);
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

        var directoryCombinedNormalized = (directoryCombined is { Length: > 0 })
            ? System.IO.Path.GetFullPath(directoryCombined)
            : directoryCombined;
        if (string.IsNullOrEmpty(directoryCombinedNormalized)) {
            return null;
        }
        if (System.IO.Directory.Exists(directoryCombinedNormalized)) {
            return directoryCombinedNormalized;
        } else {
            if (this._ConfigurationDirectoryAllowCreation) {
                System.IO.Directory.CreateDirectory(directoryCombinedNormalized);
                return directoryCombinedNormalized;
            }
            return null;
        }
        static string HandleReplacements(string value, string applicationName) {
            value = System.Environment.ExpandEnvironmentVariables(value);
            value = value.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
            value = value.Replace("{ApplicationName}", applicationName);
            return value;
        }
    }

    private readonly ConcurrentDictionary<string, TracorCollectiveFileSinkOfResourceName> _ByResourceName = new();

    private string GetGroupKey(ITracorData tracorData) {
        var result = tracorData.TracorIdentifier.RescourceName;
        if (result is { Length: > 0 }) {
            return result;
        } else {
            return this._ApplicationName;
        }
    }
    private Func<ITracorData, string>? _GetGroupKey;

    protected override async Task WriteAsync(List<ITracorData> listTracorData) {
        DateTime utcNow = DateTime.UtcNow;
        Func<ITracorData, string> getGroupKey = (this._GetGroupKey ??= ((tracorData) => this.GetGroupKey(tracorData)));
        var listTracorDataGrouped = listTracorData.GroupBy(getGroupKey);
        foreach (var groupListTracorData in listTracorDataGrouped) {
            var resourceName = groupListTracorData.Key;
            TracorCollectiveFileSinkOfResourceName? sinkByResource;

            // TryGetValue or TryAdd
            while (!this._ByResourceName.TryGetValue(resourceName, out sinkByResource)) {
                bool self = (resourceName == this._ApplicationName);
                sinkByResource = new TracorCollectiveFileSinkOfResourceName(
                    this,
                    self ? this._Resource : default,
                    resourceName,
                    this._ConfigurationDirectory ?? string.Empty);
                this._ByResourceName.TryAdd(resourceName, sinkByResource);
            }
            try {
                await sinkByResource.WriteAsync(groupListTracorData, utcNow);
            } catch (Exception ex) {
                if (this._TracorEmergencyLogging.IsEnabled) {
                    this._TracorEmergencyLogging.Log($"Error: while writing {ex.Message}");
                }
            }
        }
    }

    private static (FileStream stream, bool created) GetLogFileStream(string logFilePath) {
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

    public const string FileExtensionJsonl = ".jsonl";
    public const string FileExtensionJsonlBrotli = ".jsonl.brotli";
    public const string FileExtensionJsonlGzip = ".jsonl.gzip";
    public const string FileExtensionBrotli = ".brotli";
    public const string FileExtensionGzip = ".gzip";

    public static TracorCompression? GetCompressionFromFileName(string currentFileFQN) {
        if (currentFileFQN.EndsWith(FileExtensionJsonl)) { return TracorCompression.None; }
        if (currentFileFQN.EndsWith(FileExtensionJsonlBrotli)) { return TracorCompression.Brotli; }
        if (currentFileFQN.EndsWith(FileExtensionJsonlGzip)) { return TracorCompression.Gzip; }
        return null;
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

    internal sealed class TracorCollectiveFileSinkOfResourceName {
        private readonly TracorCollectiveFileSink _Parent;
        private readonly TracorDataRecord? _Resource;
        private readonly string _ResourceName;
        private readonly string _Directory;
        private bool _DirectoryExists;
        private DateTime _DirectoryRecheck = new DateTime(0);
        private long _PeriodStarted;

        private System.IO.Stream? _CurrentFileStream;
        internal string? _CurrentFileFQN;
        private Task _CleanupTask = Task.CompletedTask;

        public TracorCollectiveFileSinkOfResourceName(
            TracorCollectiveFileSink parent,
            TracorDataRecord? resource,
            string resourceName,
            string directory) {
            this._Parent = parent;
            this._Resource = resource;
            this._ResourceName = resourceName;
            var directoryForResource =
                directory.Contains("{Resource}")
                ? directory.Replace("{Resource}", resourceName)
                : System.IO.Path.Combine(directory, resourceName);
            this._Directory = directoryForResource;
        }

        internal void ResetForOptionsChange() {
            this._PeriodStarted = 0;
        }

        public async Task WriteAsync(IEnumerable<ITracorData> listTracorData, DateTime utcNow) {
            TimeSpan statePeriod;
            long statePeriodStarted;
            Stream? currentFileStream;
            long currentPeriodStarted;
            if (string.IsNullOrEmpty(this._Parent._ConfigurationDirectory)) {
                if (this._Parent._TracorEmergencyLogging.IsEnabled) {
                    this._Parent._TracorEmergencyLogging.Log("FileTracorCollectiveSink disabled - directory is empty");
                }
                return;
            }
            using (this._Parent._LockProperties.EnterScope()) {
                statePeriod = this._Parent._ConfigurationPeriod;
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
                                if (this._Parent._ConfigurationCompression != TracorCompression.None) {
                                    if (this._CleanupTask.IsCompleted) {
                                        this._CleanupTask = CompressFileAsync(currentFileFQN, this._Parent._ConfigurationCompression);
                                    } else {
                                        this._CleanupTask = this._CleanupTask.ContinueWith(async (_) => {
                                            await CompressFileAsync(currentFileFQN, this._Parent._ConfigurationCompression);
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
                        if (this._Parent._ConfigurationCleanupEnabled) {
                            if (12 < this._Parent._ConfigurationCleanupPeriod.TotalHours) {
                                var limit = utcNow.Subtract(this._Parent._ConfigurationCleanupPeriod);
                                if (this._CleanupTask.IsCompleted) {
                                    this._CleanupTask = this.DeleteOldLogFilesAsync(limit, directory);
                                }
                            }
                        }
                    } else {
                        try {
                            System.IO.Directory.CreateDirectory(directory);
                            this._DirectoryExists = true;
                            if (this._Parent._TracorEmergencyLogging.IsEnabled) {
                                this._Parent._TracorEmergencyLogging.Log($"{this.GetType().Name} created directory:{directory}");
                            }
                        } catch (Exception error) {
                            this._DirectoryExists = false;
                            if (this._Parent._TracorEmergencyLogging.IsEnabled) {
                                this._Parent._TracorEmergencyLogging.Log($"{this.GetType().Name} cannot create directory:{directory} {error.Message}");
                            }
                        }
                    }
                }
                if (!this._DirectoryExists) {
                    return;
                } else {
                    using (this._Parent._LockProperties.EnterScope()) {
                        var logFilePath = this.GetLogFilePath(
                            new DateTime(currentPeriodStarted * statePeriod.Ticks));
                        var logFileFQN = System.IO.Path.Combine(directory, logFilePath);
                        bool created;
                        (currentFileStream, created) = GetLogFileStream(logFileFQN);
                        this._CurrentFileStream = currentFileStream;
                        this._CurrentFileFQN = logFileFQN;
                        addResource = created;
                        addNewLine = !created;

                        this._PeriodStarted = currentPeriodStarted;

                        if (this._Parent._TracorEmergencyLogging.IsEnabled) {
                            this._Parent._TracorEmergencyLogging.Log($"{this.GetType().Name} new file:{logFileFQN}");
                        }
                    }
                }
            } else {
                addNewLine = false;
                addResource = false;
            }

            if (currentFileStream is { } currentStream) {
                await this._Parent.ConvertAndWriteAsync(
                    listTracorData,
                    addNewLine,
                    addResource ? this._Resource : default,
                    currentStream);

                await currentStream.FlushAsync();

                if (this._Parent._TracorEmergencyLogging.IsEnabled) {
                    this._Parent._TracorEmergencyLogging.Log($"{this.GetType().Name} entries flushed.");
                }
            }
        }

        public string GetLogFilePath(DateTime utcNow) {
            var timestamp = utcNow.ToString("yyyy-MM-dd-HH-mm-ss", TracorConstants.TracorCulture.DateTimeFormat);
            if (this._Parent._ConfigurationFileName is { Length: > 0 } fileName) {
                return fileName
                    .Replace("{ResourceName}", this._ResourceName)
                    .Replace("{ApplicationName}", this._Parent._ApplicationName)
                    .Replace("{TimeStamp}", timestamp)
                    ;
            } else {
                return @$"log-{this._ResourceName}-{timestamp}.jsonl";
            }
        }

        public string GetLogFilePattern() {
            if (this._Parent._ConfigurationFileName is { Length: > 0 } fileName) {
                return fileName
                    .Replace("{ResourceName}", this._ResourceName)
                    .Replace("{ApplicationName}", this._Parent._ApplicationName)
                    .Replace("{TimeStamp}", "*")
                    ;
            } else {
                //return @$"log-{this._ApplicationName}-*.jsonl";
                return @$"log-{this._ResourceName}-*.jsonl";
            }
        }

        public DateTimeOffset PeriodStarted() => new DateTimeOffset(this._PeriodStarted * this._Parent._ConfigurationPeriod.Ticks, TimeSpan.Zero);

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
                        if (this._Parent._ConfigurationCompression is TracorCompression.Brotli or TracorCompression.Gzip) {
                            await CompressFileAsync(fileInfo.FullName, this._Parent._ConfigurationCompression);
                        }
                    }
                }
            } catch (Exception error) {
                System.Console.Error.WriteLine(error.ToString());
            }
        }
    }
}