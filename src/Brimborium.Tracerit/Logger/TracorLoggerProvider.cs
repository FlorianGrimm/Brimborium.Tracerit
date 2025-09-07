namespace Brimborium.Tracerit.Logger;

[ProviderAlias("Tracor")]
public sealed class TracorLoggerProvider : ILoggerProvider, ISupportExternalScope {
    private readonly ITracor _Tracor;
    private IExternalScopeProvider? _ExternalScopeProvider;

    public TracorLoggerProvider(ITracor tracor) {
        this._Tracor = tracor;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string name) {
        return new TracorLogger(name, this._Tracor, this._ExternalScopeProvider);
    }

    /// <inheritdoc />
    public void Dispose() {
    }

    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider) {
        this._ExternalScopeProvider = scopeProvider;
    }
}
