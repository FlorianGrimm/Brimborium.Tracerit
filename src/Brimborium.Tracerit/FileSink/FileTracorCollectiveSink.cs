namespace Brimborium.Tracerit.FileSink;

public sealed class FileTracorOptions {
    public FileTracorOptions() { }

    public string? BaseDirectory { get; set; }

    public Func<string?>? GetBaseDirectory { get; set; }

    public string? Directory { get; set; } = "Logs";

    public string? FileName { get; set; } = "{AssemblyName}_{TimeStamp}.jsonl";

    public TimeSpan Period { get; set; } = TimeSpan.FromMinutes(30);

    public TimeSpan FlushPeriod { get; set; } = TimeSpan.FromSeconds(10);

    public Func<IServiceProvider, CancellationToken>? GetApplicationStopping { get; set; }
}

public sealed class FileTracorCollectiveSink : ITracorCollectiveSink, IDisposable {
    private IDisposable? _OptionsMonitorDisposing;
    private CancellationTokenRegistration? _OnApplicationStoppingDisposing;
    private readonly IServiceProvider? _ServiceProvider;

    private Lock _LockProperties = new Lock();
    private string? _Directory;
    private string? _FileName;
    private TimeSpan _Period = TimeSpan.Zero;
    private TimeSpan _FlushPeriod = TimeSpan.Zero;

    private Lock _LockBufferStream = new Lock();
    private System.Text.Json.Utf8JsonWriter? _Utf8JsonWriter;
    private System.Text.Json.JsonWriterOptions? _JsonWriterOptions;

    private System.IO.Stream? _CurrentFileStream;
    private long _PeriodStarted;

    private readonly System.Threading.Channels.Channel<ITracorData> _Channel;
    private readonly ChannelWriter<ITracorData> _ChannelWriter;
    private Task? _TaskLoop;
    private CancellationTokenSource _TaskLoopEnds = new();

    public FileTracorCollectiveSink(FileTracorOptions options) {
        this._Channel = System.Threading.Channels.Channel.CreateBounded<ITracorData>(10000);
        this._ChannelWriter = this._Channel.Writer;

        this.SetOptions(options);
    }

    public FileTracorCollectiveSink(
        IServiceProvider serviceProvider,
        IOptionsMonitor<FileTracorOptions> options) {
        this._ServiceProvider = serviceProvider;
        this._Channel = System.Threading.Channels.Channel.CreateBounded<ITracorData>(10000);
        this._ChannelWriter = this._Channel.Writer;

        this._OptionsMonitorDisposing = options.OnChange(this.SetOptions);
        this.SetOptions(options.CurrentValue);
    }

    internal void SetOptions(FileTracorOptions options) {
        using (this._LockProperties.EnterScope()) {
            if (this._ServiceProvider is { } serviceProvider
                && options.GetApplicationStopping is { } getApplicationStopping) {
                if (this._OnApplicationStoppingDisposing is { } old) {
                    old.Unregister();
                }
                var applicationStopping = getApplicationStopping(serviceProvider);
                this._OnApplicationStoppingDisposing = applicationStopping.Register(this.OnApplicationStopping);
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

    private void OnApplicationStopping() {
        this._ChannelWriter.Complete();
        this._FlushPeriod = TimeSpan.Zero;
        this.Flush();
        this.Dispose();
    }

    private void Dispose(bool disposing) {
        using (var optionsMonitorDisposing = this._OptionsMonitorDisposing) {
            using (var onApplicationStoppingDisposing = this._OnApplicationStoppingDisposing) {
                this.Flush();
                if (disposing) {
                    this._OptionsMonitorDisposing = null;
                    this._OnApplicationStoppingDisposing = null;
                }
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

    public bool IsEnabled() => true;

    private List<TracorDataProperty>? _CacheTracorDataProperties;
    private static byte[]? _ArrayNewLine;

    public void OnTrace(bool isPublic, TracorIdentitfier callee, ITracorData tracorData) {
        if (tracorData is IReferenceCountObject referenceCountObject) {
            referenceCountObject.IncrementReferenceCount();
        }
        if (this._ChannelWriter.TryWrite(tracorData)) {
            // OK
        } else {
            // channel full
        }
        if (this._TaskLoop is null) {
            this._TaskLoop = Task.Run(this.Loop).ContinueWith((_) => { this._TaskLoop = null; });
        }
    }

    private async void Loop() {
        var reader = this._Channel.Reader;
        List<ITracorData> listTracorData = new(1000);
        List<TracorDataProperty> listTracorDataProperty = new(1000);
        int watchDog = 0;
        while (!_TaskLoopEnds.IsCancellationRequested) {
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
    }

    private async Task<bool> FlushAsync(
        ChannelReader<ITracorData> reader,
        List<ITracorData> listTracorData,
        List<TracorDataProperty> listTracorDataProperty) {
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
    }

    private async Task WriterAsync(List<ITracorData> listTracorData, List<TracorDataProperty> listTracorDataProperty) {
        DateTime utcNow = DateTime.UtcNow;
        TimeSpan statePeriod;
        long statePeriodStarted;
        Stream? currentFileStream;
        Utf8JsonWriter? utf8JsonWriter;
        long currentPeriodStarted;

        using (this._LockProperties.EnterScope()) {
            statePeriod = this._Period;
            statePeriodStarted = this._PeriodStarted;
            currentFileStream = this._CurrentFileStream;
            currentPeriodStarted = utcNow.Ticks / statePeriod.Ticks;
            utf8JsonWriter = this._Utf8JsonWriter;
        }
        
        {
            if (currentFileStream is not null) {
                if (60 <= statePeriod.TotalSeconds) {
                    if (statePeriodStarted != currentPeriodStarted) {
                        if (this._Utf8JsonWriter is { } writer) {
                            await writer.FlushAsync();
                        }
                        currentFileStream.Close();
                        currentFileStream = null;
                    }
                }
            }
        }
        if (currentFileStream is null) {
            using (this._LockProperties.EnterScope()) {                
                var logFilePath = this.GetLogFilePath(new DateTime(currentPeriodStarted * statePeriod.Ticks));
                this._CurrentFileStream = currentFileStream = this.GetLogFileStream(logFilePath);
                this._PeriodStarted = currentPeriodStarted;
                if (utf8JsonWriter is null) {
                    System.Text.Json.JsonWriterOptions jsonWriterOptions;
                    if (this._JsonWriterOptions is { } options) {
                        jsonWriterOptions = options;
                    } else {
                        this._JsonWriterOptions = jsonWriterOptions = new() { };
                    }

                    this._Utf8JsonWriter = utf8JsonWriter = new System.Text.Json.Utf8JsonWriter(currentFileStream, jsonWriterOptions);
                } else {
                    utf8JsonWriter.Reset(currentFileStream);
                }                
            }
        }
        if (currentFileStream is {} && utf8JsonWriter is { }) {
            foreach (var tracorData in listTracorData) {
                byte[] nl = (_ArrayNewLine ??= Encoding.UTF8.GetBytes(System.Environment.NewLine));
                if (0 < listTracorData.Count) { listTracorData.Clear(); }
                tracorData.ConvertProperties(listTracorDataProperty);
                currentFileStream.Write(nl, 0, nl.Length);
                utf8JsonWriter.Reset(currentFileStream);
                utf8JsonWriter.WriteStartArray();

                // TODO:  use Json  converter
                foreach (var tracorDataProperty in listTracorDataProperty) {
                    utf8JsonWriter.WriteStartObject();
                    utf8JsonWriter.WriteString("name", tracorDataProperty.Name);
                    utf8JsonWriter.WriteString("name", tracorDataProperty.TypeName);
                    utf8JsonWriter.WriteString("text_Value", tracorDataProperty.TextValue);
                    utf8JsonWriter.WriteEndObject();

                }
                utf8JsonWriter.WriteEndArray();
                await utf8JsonWriter.FlushAsync();
                listTracorData.Clear();
            }
        }
    }


#if false
    public void OnTrace(bool isPublic, TracorIdentitfier callee, ITracorData tracorData) {
        using (this._LockBufferStream.EnterScope()) {
            var listTracorDataProperties = System.Threading.Interlocked.Exchange(ref _CacheTracorDataProperties, default) ?? new(128);
            callee.ConvertProperties(listTracorDataProperties);
            tracorData.ConvertProperties(listTracorDataProperties);
            this._Utf8JsonWriter.WriteStartArray();
            foreach (var property in listTracorDataProperties) {
                this._Utf8JsonWriter.WriteStartArray();
                this._Utf8JsonWriter.WriteStringValue(property.Name);
                this._Utf8JsonWriter.WriteStringValue(property.TypeName);
                this._Utf8JsonWriter.WriteStringValue(property.TextValue);
                this._Utf8JsonWriter.WriteEndArray();
            }
            this._Utf8JsonWriter.WriteEndArray();
            this._Utf8JsonWriter.Flush();
            this._Utf8JsonWriter.Reset();

            System.Threading.Interlocked.Exchange(ref _CacheTracorDataProperties, listTracorDataProperties);
        }

        {
            var utcNow = System.DateTime.UtcNow;
            var periodTicks = this._Period.Ticks;
            if (periodTicks <= 60) {
                var logFilePath = this.GetLogFilePath(utcNow);
                this._CurrentFileStream = this.GetLogFileStream(logFilePath);
            } else {
                var periodStarted = utcNow.Ticks / this._Period.Ticks;
                {
                    if (this._CurrentFileStream is { } currentFileStream) {
                        // check for the next period
                        if (this._PeriodStarted != periodStarted) {
                            using (this._LockBufferStream.EnterScope()) {
                                this._CurrentBufferStream.CopyTo(currentFileStream);
                                this._CurrentBufferStream.Position = 0;
                                this._CurrentBufferStream.SetLength(0);
                                currentFileStream.Flush();
                                currentFileStream.Dispose();
                                this._CurrentFileStream = null;
                            }
                        }
                    } else {
                        this._PeriodStarted = periodStarted;
                    }
                }
                {
                    var logFilePath = this.GetLogFilePath(utcNow);
                    this._CurrentFileStream = this.GetLogFileStream(logFilePath);

                }
            }
        }
    }
#endif
    private string GetLogFilePath(DateTime utcNow) {
        var timestamp = utcNow.ToString("yyyy-MM-dd-HH-mm", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        return @$"c:\temp\log-{timestamp}.jsonl";
    }


    private Stream? GetLogFileStream(string logFilePath) {
        if (System.IO.File.Exists(logFilePath)) {
            return new System.IO.FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        } else {
            return new System.IO.FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
    }

    private void Flush() {
        throw new NotImplementedException();
    }

}
