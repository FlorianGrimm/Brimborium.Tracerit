namespace Brimborium.Tracerit.Service;

internal sealed class RuntimeTracor : ITracor {
    public bool IsGeneralEnabled() => false;

    public bool IsCurrentlyEnabled() => false;

    public void Trace<T>(TracorIdentitfier callee, T value) {
        // this is should not be called, but anyway...
        if (value is IDisposable valueDisposable) {
            valueDisposable.Dispose();
        }
    }
}
