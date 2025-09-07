namespace Brimborium.Tracerit.Service;

public sealed class TracorGlobalState : Dictionary<string, object> {
    public TracorGlobalState() {
    }
    public TracorGlobalState(Dictionary<string, object> src) : base(src) {
    }
    public TracorGlobalState SetValue<T>(string key, T value) where T:notnull {
        this[key] = value;
        return this;
    }
    public T GetValue<T>(string key) => (T)this[key];
}
