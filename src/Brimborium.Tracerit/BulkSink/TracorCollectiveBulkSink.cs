using System.IO.Compression;

namespace Brimborium.Tracerit.BulkSink;

public abstract class TracorCollectiveBulkSink<TOptions>
    : ITracorCollectiveSink, IDisposable
    where TOptions : TracorBulkSinkOptions {
    protected IDisposable? _TracorOptionsMonitorDisposing;
    protected IDisposable? _FileTracorOptionsMonitorDisposing;
    protected readonly IServiceProvider? _ServiceProvider;
    protected CancellationTokenRegistration? _OnApplicationStoppingDisposing;
    protected Func<CancellationToken?> _GetOnApplicationStoppingDisposing = () => null;

    protected readonly Lock _LockProperties = new Lock();
    protected readonly SemaphoreSlim _AsyncLockWriteFile = new(initialCount: 1, maxCount: 1);
    protected TimeSpan _FlushPeriod = TimeSpan.Zero;
    protected TracorDataRecord? _Resource;

    protected readonly System.Threading.Channels.Channel<ITracorData> _Channel;
    protected readonly ChannelWriter<ITracorData> _ChannelWriter;
    protected Task? _TaskLoop;
    protected readonly CancellationTokenSource _TaskLoopEnds = new();
    protected readonly TracorEmergencyLogging _TracorEmergencyLogging;
    protected string? _ApplicationName;

    protected readonly TracorPropertySinkTargetPool _Pool;
    protected System.Text.Json.JsonSerializerOptions? _JsonSerializerOptions;
    protected System.IO.Stream? _CurrentStream;

    protected TracorCollectiveBulkSink(
        TracorOptions tracorOptions,
        TOptions bulkSinkOptions
        ) : this(tracorOptions, bulkSinkOptions, new()) {
    }

    protected TracorCollectiveBulkSink(
        TracorOptions tracorOptions,
        TOptions bulkSinkOptions,
        TracorEmergencyLogging tracorEmergencyLogging) {
        this._Channel = System.Threading.Channels.Channel.CreateBounded<ITracorData>(10000);
        this._ChannelWriter = this._Channel.Writer;
        this._TracorEmergencyLogging = tracorEmergencyLogging;
        this._Pool = new();

        this.SetTracorOptions(tracorOptions);
        this.SetBulkSinkOptions(bulkSinkOptions);
    }

    protected TracorCollectiveBulkSink(
        IServiceProvider serviceProvider,
        IOptionsMonitor<TracorOptions> tracorOptions,
        IOptionsMonitor<TracorBulkSinkOptions> bulkSinkOptions,
        TracorEmergencyLogging tracorEmergencyLogging) {
        this._ServiceProvider = serviceProvider;
        this._Channel = System.Threading.Channels.Channel.CreateBounded<ITracorData>(10000);
        this._ChannelWriter = this._Channel.Writer;
        this._TracorEmergencyLogging = tracorEmergencyLogging;
        this._Pool = new();

        this._TracorOptionsMonitorDisposing = tracorOptions.OnChange(this.SetTracorOptions);
        this._FileTracorOptionsMonitorDisposing = bulkSinkOptions.OnChange(this.SetTracorBulkSinkOptions);
        this.SetTracorOptions(tracorOptions.CurrentValue);
        this.SetBulkSinkOptions((TOptions)bulkSinkOptions.CurrentValue);
    }

    private void SetTracorBulkSinkOptions(TracorBulkSinkOptions options, string? name) {
        this.SetBulkSinkOptions((TOptions)options);
    }

    protected virtual void SetTracorOptions(TracorOptions tracorOptions) {
        using (this._LockProperties.EnterScope()) {
            if (tracorOptions.ApplicationName is { Length: > 0 } applicationName) {
                string machineName = System.Environment.MachineName;
                this._ApplicationName = applicationName
                    .Replace("{MaschineName}", machineName);
            } else if (this._ApplicationName is null) {
                string machineName = System.Environment.MachineName;
                applicationName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Application";
                applicationName = $"{applicationName}-{machineName}";
                this._ApplicationName = applicationName;
            }

            if (this._ServiceProvider is { } serviceProvider
                && tracorOptions.GetOnGetApplicationStopping() is { } getApplicationStopping) {
                // using the callback avoids 
                this._GetOnApplicationStoppingDisposing = (() => getApplicationStopping(serviceProvider));
            }
        }
    }

    internal void SetBulkSinkOptions(TOptions options) {
        using (this._LockProperties.EnterScope()) {
            this._JsonSerializerOptions = new System.Text.Json.JsonSerializerOptions(options.GetJsonSerializerOptions());
            this.SetBulkSinkOptionsExtended(options);
        }
    }

    internal virtual void SetBulkSinkOptionsExtended(TOptions options) { }

    protected virtual System.Text.Json.JsonSerializerOptions GetJsonSerializerOptions() {
        if (this._JsonSerializerOptions == null) {
            this._JsonSerializerOptions =
                TracorDataSerialization.AddTracorDataMinimalJsonConverter(null, null);
        }
        return this._JsonSerializerOptions;
    }

    protected void OnApplicationStopping() {
        this._ChannelWriter.Complete();
        this._FlushPeriod = TimeSpan.Zero;
        var _ = this.FlushAsync().ContinueWith((t) => { this.Dispose(); });
    }

    protected void Dispose(bool disposing) {
        using (var tracorOptionsMonitorDisposing = this._TracorOptionsMonitorDisposing) {
            using (var optionsMonitorDisposing = this._FileTracorOptionsMonitorDisposing) {
                using (var onApplicationStoppingDisposing = this._OnApplicationStoppingDisposing) {
                    if (disposing) {
                        this._TracorOptionsMonitorDisposing = null;
                        this._FileTracorOptionsMonitorDisposing = null;
                        this._OnApplicationStoppingDisposing = null;
                    }
                    this.Flush();
                }
            }
        }
    }

    ~TracorCollectiveBulkSink() {
        this.Dispose(disposing: false);
    }

    public void Dispose() {
        this.Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }

    public virtual bool IsGeneralEnabled() => true;

    public virtual bool IsEnabled() => true;

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

    protected async Task Loop() {
        using (this._LockProperties.EnterScope()) {
            if (this._GetOnApplicationStoppingDisposing() is { } applicationStopping) {
                this._OnApplicationStoppingDisposing = applicationStopping.Register(this.OnApplicationStopping);
            }
        }

        var reader = this._Channel.Reader;
        List<ITracorData> listTracorData = new(1000);
        try {
            while (!this._TaskLoopEnds.IsCancellationRequested) {
                bool entriesWritten = await this.FlushAsync(reader, listTracorData);
                if (entriesWritten) {
                    if (this._TracorEmergencyLogging.IsEnabled) {
                        this._TracorEmergencyLogging.Log($"{this.GetType().Name} {listTracorData.Count} entries written.");
                    }
                    listTracorData.Clear();
                    // this will take normally a litte time - so loop after wait.
                    if (TimeSpan.Zero != this._FlushPeriod) {
                        await Task.Delay(this._FlushPeriod);
                    }
                    continue;
                } else {
                    if (TimeSpan.Zero != this._FlushPeriod) {
                        await Task.Delay(this._FlushPeriod);
                    }
                    {
                        await reader.WaitToReadAsync(this._TaskLoopEnds.Token);
                        continue;
                    }
                }
            }
            await this.FlushAsync(reader, listTracorData);
        } catch (System.Exception error) {
            System.Console.Error.WriteLine(error.ToString());
        }
    }

    /// <summary>
    /// Transport from source to target
    /// </summary>
    /// <param name="reader">the source</param>
    /// <param name="listTracorData">the target - avoid GC - must be empty and will be empty after</param>
    /// <returns>true - if some records are written </returns>
    protected async Task<bool> FlushAsync(
        ChannelReader<ITracorData> reader,
        List<ITracorData> listTracorData) {
        await this._AsyncLockWriteFile.WaitAsync();
        try {
            while (reader.TryRead(out var tracorData)) {
                listTracorData.Add(tracorData);
            }
            if (0 == listTracorData.Count) {
                return false;
            }

            {
                await this.WriteAsync(listTracorData);
                return true;
            }
        } finally {
            this._AsyncLockWriteFile.Release();
        }
    }

    protected abstract Task WriteAsync(List<ITracorData> listTracorData);

    private readonly byte[] _ArrayNewLine = Encoding.UTF8.GetBytes(System.Environment.NewLine);

    protected virtual async Task ConvertAndWriteAsync(
        IEnumerable<ITracorData> listTracorData,
        bool reuseStream,
        TracorDataRecord? addResource,
        Stream currentStream
        ) {
        var jsonSerializerOptions = this.GetJsonSerializerOptions();

        byte[] newLine = this._ArrayNewLine;

        if (reuseStream) {
            currentStream.Write(newLine, 0, newLine.Length);
        }
        if (addResource is { } ) {
            System.Text.Json.JsonSerializer.Serialize(currentStream, addResource, jsonSerializerOptions);
            currentStream.Write(newLine, 0, newLine.Length);
        }

        foreach (var tracorData in listTracorData) {
            var propertySinkTarget = this._Pool.Rent();
            // tracorData.ConvertProperties(propertySinkTarget.ListProperty);
            // System.Text.Json.JsonSerializer.Serialize(currentStream, tracorData, jsonSerializerOptions);
            // currentStream.Write(newLine, 0, newLine.Length);

            // Use the extension method that fills in the default application name if not set
            tracorData.CopyPropertiesToSink(propertySinkTarget, this._ApplicationName);
            System.Text.Json.JsonSerializer.Serialize(currentStream, propertySinkTarget, jsonSerializerOptions);
            currentStream.Write(newLine, 0, newLine.Length);

            if (tracorData is IReferenceCountObject referenceCountObject) {
                referenceCountObject.Dispose();
            }
            propertySinkTarget.Dispose();
        }

        await currentStream.FlushAsync();
    }

    protected void Flush() {
        this.FlushAsync().GetAwaiter().GetResult();
    }

    public async Task FlushAsync() {
        var reader = this._Channel.Reader;
        List<ITracorData> listTracorData = new(1000);
        await this.FlushAsync(reader, listTracorData);
    }
}