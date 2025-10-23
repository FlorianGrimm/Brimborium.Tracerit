namespace Brimborium.Tracerit.Service;

/// <summary>
/// Runtime implementation of <see cref="ITracorServiceSink"/> that disables tracing for production scenarios.
/// This implementation provides a no-op tracer that disposes of disposable values but performs no actual tracing.
/// </summary>
internal sealed class DisabledTracorServiceSink : ITracorServiceSink {

    public ITracorScopedFilterFactory GetTracorScopedFilterFactory() 
        => NullTracorScopedFilterFactory.Instance;

    /// <summary>
    /// Determines if tracing is generally enabled at the configuration level.
    /// </summary>
    /// <returns>Always returns false for runtime scenarios.</returns>
    public bool IsGeneralEnabled() => false;

    /// <summary>
    /// Determines if tracing is currently enabled and active for processing.
    /// </summary>
    /// <returns>Always returns false for runtime scenarios.</returns>
    public bool IsCurrentlyEnabled() => false;

    public bool IsPrivateEnabled(string scope, LogLevel logLevel) => false;

    public bool IsPublicEnabled(string scope, LogLevel logLevel) => false;

    public bool HasSubScope(string scope) => false;

    public void TracePrivate<T>(string scope, LogLevel level, string message, T value) { }

    public void TracePublic<T>(string scope, LogLevel level, string message, T value) { }

    /*
    private static NullTracorSink? _NullTracorSink;
    private class NullTracorSink : ITracorSink {
        bool ITracorSink.IsPrivateEnabled(string scope, LogLevel level) => false;

        bool ITracorSink.IsPublicEnabled(string scope, LogLevel level) => false;

        void ITracorSink.TracePrivate<T>(string scope, LogLevel level, string message, T value) { }

        void ITracorSink.TracePublic<T>(string scope, LogLevel level, string message, T value) { }
    }
    */
}
