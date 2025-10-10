namespace Brimborium.Tracerit.Logger;

/// <summary>
/// Provides logger instances that integrate with the Tracor tracing system.
/// This provider creates <see cref="TracorLogger"/> instances that can capture logging events as trace data.
/// </summary>
[ProviderAlias("Tracor")]
public sealed class TracorLoggerProvider : ILoggerProvider, ISupportExternalScope {
    private readonly Lock _Lock = new();
    private ITracorCollectivePublisher? _Publisher;
    private readonly IServiceProvider _ServiceProvider;
    private readonly LogLevel? _MinimumLogLevel;
    private IExternalScopeProvider? _ExternalScopeProvider;

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
        using (this._Lock.EnterScope()) {
            publisher = this._Publisher;
            if (publisher is null) {
                this._Publisher = publisher = this._ServiceProvider.GetRequiredService<ITracorCollectivePublisher>();
            }
        }
        if (publisher.IsEnabled()) {
            return new TracorLogger(name, publisher, this._MinimumLogLevel, this._ExternalScopeProvider);
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
