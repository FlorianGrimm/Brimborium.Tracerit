namespace Brimborium.Tracerit.Service;

public sealed class TracorForkState : Dictionary<string, object> {
    private readonly Dictionary<string, IEqualityComparer> _DictEqualityComparer = [];

    public TracorForkState() {
    }

    public TracorForkState(TracorForkState src) {
        foreach (var kv in src) {
            this[kv.Key] = kv.Value;
            if (src._DictEqualityComparer.TryGetValue(kv.Key, out var equalityComparer)) {
                this._DictEqualityComparer[kv.Key] = equalityComparer;
            }
        }
    }

    public void SetItem(string propertyName, object propertyValue, IEqualityComparer equalityComparer) {
        this[propertyName] = propertyValue;
        this._DictEqualityComparer[propertyName] = equalityComparer;
    }

    public bool IsPartalEqual(TracorForkState biggerState) {
        foreach (var kv in this) {
            if (biggerState.TryGetValue(kv.Key, out var value)) {
                if (ReferenceEquals(kv.Value, value)) {
                    continue;
                }
                if (kv.Value is null || value is null) {
                    return false;
                }
                var isEqual = this._DictEqualityComparer.TryGetValue(kv.Key, out var equalityComparer)
                    ? equalityComparer.Equals(kv.Value, value)
                    : kv.Value.Equals(value);
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
