namespace Brimborium.Tracerit.Service;

internal sealed class RuntimeTracor : ITracor {
    public bool IsGeneralEnabled() => false;

    public bool IsCurrentlyEnabled() => false;

    public void Trace<T>(TracorIdentitfier callee, T Value) { }
}
