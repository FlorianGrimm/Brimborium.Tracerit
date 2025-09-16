namespace Brimborium.Tracerit.Logger;

/// <summary>
/// Provides logger instances that integrate with the Tracor tracing system.
/// This provider creates <see cref="TracorLogger"/> instances that can capture logging events as trace data.
/// </summary>
[ProviderAlias("Tracor")]
public sealed class TracorLoggerProvider : ILoggerProvider, ISupportExternalScope {
    private readonly Lock _Lock = new();
    private ITracor? _Tracor;
    private readonly IServiceProvider _ServiceProvider;
    private IExternalScopeProvider? _ExternalScopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorLoggerProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the <see cref="ITracor"/> instance.</param>
    public TracorLoggerProvider(IServiceProvider serviceProvider) {
        this._ServiceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string name)
    {
        using (this._Lock.EnterScope()) {
            if (this._Tracor is null) {
                this._Tracor = this._ServiceProvider.GetRequiredService<ITracor>();
            }
        }
        if (this._Tracor.IsGeneralEnabled()) {
            return new TracorLogger(name, this._Tracor, this._ExternalScopeProvider);
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
