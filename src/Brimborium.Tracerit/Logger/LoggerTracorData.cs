#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Brimborium.Tracerit.DataAccessor;

public sealed class LoggerTracorData : ReferenceCountObject, ITracorData {
    private readonly List<KeyValuePair<string, object?>> _Arguments;

    public LoggerTracorData(IReferenceCountPool? referenceCountPool) : base(referenceCountPool) {
        this._Arguments = new(128);
    }

    protected override void ResetState() {
        this.Arguments.Clear();
    }

    protected override bool IsStateReset() => 0 == this.Arguments.Count && this.Arguments.Capacity <= 128;

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

    public bool DoesMatch(string? source, string? eventName) {
        (bool isSourceFound, bool sourceMatched, string sourceValue) = (source is { Length: > 0 } valueSource) ? (false, false, valueSource) : (true, true, string.Empty);
        (bool isEventNameFound, bool eventNameMatched, string eventNameValue) = (eventName is { Length: > 0 } valueEventName) ? (false, false, valueEventName) : (true, true, string.Empty);

        foreach (var arg in this._Arguments) {
            if (!isSourceFound) {
                if (TracorConstants.TracorDataPropertyNameSource == arg.Key) {
                    isSourceFound = true;
                    sourceMatched = string.Equals(sourceValue, arg.Value as string, StringComparison.Ordinal);
                    if (isEventNameFound) { break; }
                }
            }
            if (!isEventNameFound) {
                if (TracorConstants.TracorDataPropertyNameEventName == arg.Key) {
                    isEventNameFound = true;
                    eventNameMatched = string.Equals(eventNameValue, arg.Value as string, StringComparison.Ordinal);
                    if (isSourceFound) { break; }
                }
            }
        }
        return (sourceMatched && eventNameMatched);
    }

    /// <summary>
    /// Gets or sets the identifier associated with this trace data record.
    /// </summary>
    public TracorIdentifier TracorIdentifier { get; set; }

    public DateTime Timestamp { get; set; }

    public bool TryGetDataProperty(string propertyName, out TracorDataProperty result) {
        foreach (var arg in this._Arguments) {
            if (string.Equals(arg.Key, propertyName, StringComparison.Ordinal)) {
                result = TracorDataProperty.Create(arg.Key, arg.Value);
                return true;
            }
        }

        result = new TracorDataProperty(string.Empty);
        return false;
    }

    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        foreach (var arg in this._Arguments) {
            if (arg.Value is { } argValue) {
                listProperty.Add(TracorDataProperty.Create(arg.Key, argValue));
            }
        }
    }
}