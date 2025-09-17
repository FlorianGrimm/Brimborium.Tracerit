namespace Brimborium.Tracerit.TracorActivityListener;

/// <summary>
/// do nothing
/// </summary>
internal sealed class RuntimeTracorActivityListener : ITracorActivityListener {
    public void AddActivitySourceName(string name) {
        // do nothing
    }

    public void AddListInstrumentation(ActivitySourceIdenifier instrumentationRegistration) {
        // do nothing
    }

    public void RemoveActivitySourceName(string name) {
        // do nothing
    }

    public void RemoveListInstrumentation(ActivitySourceIdenifier instrumentationRegistration) {
        // do nothing
    }

    public void Start() {
        // do nothing
    }

    public void Stop() {
        // do nothing
    }
}
