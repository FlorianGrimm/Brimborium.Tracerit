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
    /// <summary>
    /// Creates a new <see cref="TracorLogger"/> instance with the specified name.
    /// </summary>
    /// <param name="name">The name of the logger.</param>
    /// <returns>A new <see cref="TracorLogger"/> instance.</returns>
    public ILogger CreateLogger(string name)
    {
        using (this._Lock.EnterScope()) {
            if (this._Tracor is null) {
                this._Tracor = this._ServiceProvider.GetRequiredService<ITracor>();
            }
        }
        return new TracorLogger(name, this._Tracor, this._ExternalScopeProvider);
    }

    /// <inheritdoc />
    /// <summary>
    /// Disposes the logger provider. This implementation does not require cleanup.
    /// </summary>
    public void Dispose() {
    }

    /// <inheritdoc />
    /// <summary>
    /// Sets the external scope provider for managing logging scopes.
    /// </summary>
    /// <param name="scopeProvider">The external scope provider to use.</param>
    public void SetScopeProvider(IExternalScopeProvider scopeProvider) {
        this._ExternalScopeProvider = scopeProvider;
    }
}
