namespace Brimborium.Tracerit.Service;

public sealed class TracorForkState : Dictionary<string, TracorDataProperty> {
    public readonly Lock Lock = new Lock();
    public TracorForkState() {
    }

    public TracorForkState(TracorForkState src) {
        foreach (var kv in src) {
            this[kv.Key] = kv.Value;
        }
    }

    public TracorForkState SetValue(TracorDataProperty propertyValue) {
        this[propertyValue.Name] = propertyValue;
        return this;
    }

    public TracorDataProperty GetValue(string name) {
        if (this.TryGetValue(name, out var value)) {
            return value;
        } else {
            return new TracorDataProperty(name);
        }
    }

    public bool IsPartialEqual(TracorForkState biggerState) {
        foreach (var kv in this) {
            if (biggerState.TryGetValue(kv.Key, out var value)) {
                var isEqual = TracorDataPropertyValueEqualityComparer.Default.Equals(value, kv.Value);
                if (!isEqual) {
                    return false;
                }
            } else {
                return false;
            }
        }
        return true;
    }
}
