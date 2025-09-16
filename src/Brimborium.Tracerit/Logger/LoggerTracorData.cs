using Brimborium.Tracerit.Utility;

namespace Brimborium.Tracerit.Logger;

public sealed class LoggerTracorData : ReferenceCountObject, ITracorData {
    private readonly List<KeyValuePair<string, object?>> _Arguments;

    public LoggerTracorData(IReferenceCountPool? referenceCountPool) : base(referenceCountPool) {
        this._Arguments = new(128);
    }

    protected override void ResetState() {
        this.Arguments.Clear();
    }

    protected override bool IsStateReseted() => 0 == this.Arguments.Count && this.Arguments.Capacity <= 128;

    public List<KeyValuePair<string, object?>> Arguments => this._Arguments;

    public int Index { get; set; }

    public object? this[string propertyName] {
        get {
            if (this.TryGetPropertyValue(propertyName, out var propertyValue)) {
                return propertyValue;
            } else {
                return null;
            }
        }
    }

    public List<string> GetListPropertyName() {
        return this._Arguments.Select(i => i.Key).ToList();
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        foreach (var arg in this._Arguments) {
            if (arg.Key == propertyName) {
                propertyValue = arg.Value;
                return true;
            }
        }
        propertyValue = null;
        return false;
    }
}
