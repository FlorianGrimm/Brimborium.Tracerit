#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Brimborium_Tracerit_Logger;

// Simple mock implementation for testing
internal class MockTracor : ITracor, IServiceProvider {
    public bool GeneralEnabled { get; set; } = true;
    public bool CurrentlyEnabled { get; set; } = true;
    public List<(TracorIdentitfier callee, object value)> TraceCalls { get; } = new();

    public bool IsGeneralEnabled() => this.GeneralEnabled;
    public bool IsCurrentlyEnabled() => this.CurrentlyEnabled;

    public void Trace<T>(TracorIdentitfier callee, T value) {
        this.TraceCalls.Add((callee, value!));
    }

    public object? GetService(Type serviceType)
    {
        if (typeof(ITracor).Equals(serviceType)) {
            return this;
        }
        throw new NotSupportedException("");
    }
}

internal class MockExternalScopeProvider : IExternalScopeProvider {
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
        return new MockScopeDisposable(()=> { });
    }
}

internal class MockScopeDisposable : IDisposable {
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
