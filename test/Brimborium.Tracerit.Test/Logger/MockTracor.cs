#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Brimborium_Tracerit_Logger;

// Simple mock implementation for testing
internal sealed class MockTracor : ITracor, IServiceProvider {
    public List<(TracorIdentitfier callee, LogLevel level, object value)> TraceCalls { get; } = new();

    public bool GeneralEnabled { get; set; } = true;

    public bool CurrentlyEnabled { get; set; } = true;

    public Func<LogLevel, bool> IsEnabled { get; set; } = (_) => true;

    public Func<LogLevel, TracorLevel>? GetTracorEnabled { get; set; }

    public Func<Type, object?>? GetService { get; set; }

    object? IServiceProvider.GetService(Type serviceType) {
        if (typeof(ITracor).Equals(serviceType)) {
            return this;
        }
        if (this.GetService is { }) {
            return this.GetServices(serviceType);
        }
        throw new NotSupportedException("");
    }

    TracorLevel ITracor.GetPrivateTracorEnabled(LogLevel logLevel) {
        if (this.GetTracorEnabled is { }) {
            return this.GetTracorEnabled(logLevel);
        }
        return new TracorLevel(false, _NullTracorSink ??= new());
    }

    TracorLevel ITracor.GetPublicTracorEnabled(LogLevel logLevel) {
        if (this.GetTracorEnabled is { }) {
            return this.GetTracorEnabled(logLevel);
        }
        return new TracorLevel(false, _NullTracorSink ??= new());
    }

    bool ITracor.IsGeneralEnabled() => this.GeneralEnabled;

    bool ITracor.IsCurrentlyEnabled() => this.CurrentlyEnabled;

    bool ITracor.IsPrivateEnabled(LogLevel logLevel) => this.IsEnabled(logLevel);

    bool ITracor.IsPublicEnabled(LogLevel logLevel) => this.IsEnabled(logLevel);

    void ITracorSink.TracePrivate<T>(TracorIdentitfier callee, LogLevel level, T value) {
        this.TraceCalls.Add((callee, level, value!));
    }

    void ITracorSink.TracePublic<T>(TracorIdentitfier callee, LogLevel level, T value) {
        this.TraceCalls.Add((callee, level, value!));
    }

    private static NullTracorSink? _NullTracorSink;
    private class NullTracorSink : ITracorSink {
        public void TracePrivate<T>(TracorIdentitfier callee, LogLevel level, T Value) { }

        public void TracePublic<T>(TracorIdentitfier callee, LogLevel level, T Value) { }
    }
}

internal sealed class MockExternalScopeProvider : IExternalScopeProvider {
    public List<object> PushedScopes { get; } = new();

    public void ForEachScope<TState>(Action<object?, TState> callback, TState state) {
        foreach (var scope in this.PushedScopes) {
            callback(scope, state);
        }
    }

    public IDisposable Push(object? state) {
        if (state != null) {
            this.PushedScopes.Add(state);
            return new MockScopeDisposable(() => this.PushedScopes.Remove(state));
        }
        return new MockScopeDisposable(() => { });
    }
}

internal sealed class MockScopeDisposable : IDisposable {
    private readonly Action _OnDispose;
    private bool _Disposed;

    public MockScopeDisposable(Action onDispose) {
        this._OnDispose = onDispose;
    }

    public void Dispose() {
        if (!this._Disposed) {
            this._OnDispose();
            this._Disposed = true;
        }
    }
}
