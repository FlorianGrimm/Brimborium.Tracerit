namespace Brimborium.Tracerit.TracorActivityListener;

/// <summary>
/// do nothing
/// </summary>
internal sealed class RuntimeTracorActivityListener : ITracorActivityListener {
    public void AddActivitySourceName(string name) {
        // do nothing
    }

    public void AddListInstrumentation(ActivitySourceIdentifier activitySourceIdentifier) {
        // do nothing
    }

    public void RemoveActivitySourceName(string name) {
        // do nothing
    }

    public void RemoveListInstrumentation(ActivitySourceIdentifier activitySourceIdentifier) {
        // do nothing
    }

    public void Start() {
        // do nothing
    }

    public void Stop() {
        // do nothing
    }
}
