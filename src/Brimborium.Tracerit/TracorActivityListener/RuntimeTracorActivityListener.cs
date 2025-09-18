namespace Brimborium.Tracerit.TracorActivityListener;

/// <summary>
/// do nothing
/// </summary>
internal sealed class RuntimeTracorActivityListener : ITracorActivityListener {
    public void AddActivitySourceName(string name) {
        // do nothing
    }

    public void AddActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier) {
        // do nothing
    }

    public void RemoveActivitySourceName(string name) {
        // do nothing
    }

    public void RemoveActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier) {
        // do nothing
    }

    public void AddActivitySourceByType<T>() where T : ActivitySourceBase {
        // do nothing
    }

    public void RemoveActivitySourceByType<T>() where T : ActivitySourceBase {
        // do nothing
    }

    public void Start() {
        // do nothing
    }

    public void Stop() {
        // do nothing
    }
}
