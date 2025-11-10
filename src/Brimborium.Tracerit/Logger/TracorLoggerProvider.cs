namespace Brimborium.Tracerit.Logger;

/// <summary>
/// Provides logger instances that integrate with the Tracor tracing system.
/// This provider creates <see cref="TracorLogger"/> instances that can capture logging events as trace data.
/// </summary>
[ProviderAlias("Tracor")]
public sealed class TracorLoggerProvider : ILoggerProvider, ISupportExternalScope {
    private ITracorCollectivePublisher? _Publisher;
    private ITracorDataConvertService? _DataConvertService;
    private readonly IServiceProvider _ServiceProvider;
    private readonly LogLevel? _MinimumLogLevel;
    private IExternalScopeProvider? _ExternalScopeProvider;
    private TracorDataRecordPool? _TracorDataRecordPool;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorLoggerProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the <see cref="ITracorServiceSink"/> instance.</param>
    public TracorLoggerProvider(
        IServiceProvider serviceProvider,
        IOptions<TracorLoggerOptions> options) {
        this._ServiceProvider = serviceProvider;
        this._MinimumLogLevel = options.Value.LogLevel;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string name) {
        var publisher = this._Publisher;
        var dataConvertService = this._DataConvertService;
        var tracorDataRecordPool = this._TracorDataRecordPool;
        if (publisher is null) {
            publisher = this._ServiceProvider.GetRequiredService<ITracorCollectivePublisher>();
            this._Publisher = publisher;
        }
        if (dataConvertService is null) {
            dataConvertService = this._ServiceProvider.GetRequiredService<ITracorDataConvertService>();
            this._DataConvertService = dataConvertService;
        }
        if (tracorDataRecordPool is null) {
            tracorDataRecordPool = this._ServiceProvider.GetRequiredService<TracorDataRecordPool>();
            this._TracorDataRecordPool = tracorDataRecordPool;
        }
        if (publisher.IsEnabled()) {
            return new TracorLogger(
                name,
                tracorDataRecordPool, dataConvertService, publisher,
                this._MinimumLogLevel, this._ExternalScopeProvider);
        } else {
            return new TracorDisabledLogger();
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        this._ExternalScopeProvider = null;
    }

    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider) {
        this._ExternalScopeProvider = scopeProvider;
    }
}
