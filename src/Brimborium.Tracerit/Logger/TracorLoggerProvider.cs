namespace Brimborium.Tracerit.Logger;

/// <summary>
/// Provides logger instances that integrate with the Tracor tracing system.
/// This provider creates <see cref="TracorLogger"/> instances that can capture logging events as trace data.
/// </summary>
[ProviderAlias("Tracor")]
public sealed class TracorLoggerProvider : ILoggerProvider, ISupportExternalScope {
    private readonly Lock _Lock = new();
    private ITracorServiceSink? _Tracor;
    private ITracorValidator? _TracorValidator;
    private readonly IServiceProvider _ServiceProvider;
    private readonly LogLevel? _GlobalLogLevel;
    private IExternalScopeProvider? _ExternalScopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorLoggerProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the <see cref="ITracorServiceSink"/> instance.</param>
    public TracorLoggerProvider(
        IServiceProvider serviceProvider,
        IOptions<TracorLoggerOptions> options) {
        this._ServiceProvider = serviceProvider;
        this._GlobalLogLevel = options.Value.LogLevel;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string name) {
        using (this._Lock.EnterScope()) {
            if (this._Tracor is null) {
                this._Tracor = this._ServiceProvider.GetRequiredService<ITracorServiceSink>();
            }
            if (this._TracorValidator is null) {
                this._TracorValidator = this._ServiceProvider.GetRequiredService<ITracorValidator>();
            }
        }
        if (this._Tracor.IsGeneralEnabled()) {
            return new TracorLogger(name, this._Tracor, this._TracorValidator, this._GlobalLogLevel, this._ExternalScopeProvider);
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
