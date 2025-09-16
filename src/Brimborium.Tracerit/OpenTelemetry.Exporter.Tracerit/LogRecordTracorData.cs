using OpenTelemetry.Logs;

namespace OpenTelemetry.Exporter.Tracerit;

public sealed class LogRecordTracorData
    : ITracorData<LogRecord> {
    private readonly LogRecord _LogRecord;

    public LogRecordTracorData(LogRecord logRecord) {
        this._LogRecord = logRecord;
    }

    private static string[] _StandardNames = [
        nameof(LogRecord.CategoryName),
        "Event.Id",
        "Event.Name",
        nameof(LogRecord.Severity),
        nameof(LogRecord.SeverityText)
        ];
    public List<string> GetListPropertyName() {
        List<string> result;
        if (this._LogRecord.Attributes is { } attributes) {
            result = new(attributes.Count + 5);
            result.AddRange(_StandardNames);
            foreach (var attribute in attributes) {
                result.Add(attribute.Key);
            }
        } else {
            result = new(5);
            result.AddRange(_StandardNames);
        }
        return result;
    }

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out LogRecord value) {
        value = this._LogRecord;
        return true;
    }
    public object? this[string propertyName] => this.TryGetPropertyValue(propertyName, out var result) ? result : null;

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        if (nameof(LogRecord.CategoryName) == propertyName) {
            propertyValue = this._LogRecord.CategoryName;
            return true;
        }
        if ("Event.Id" == propertyName) {
            propertyValue = this._LogRecord.EventId.Id;
            return true;
        }
        if ("Event.Name" == propertyName) {
            propertyValue = this._LogRecord.EventId.Name;
            return true;
        }
        if (nameof(LogRecord.Severity) == propertyName) {
            propertyValue = this._LogRecord.Severity;
            return true;
        }
        if (nameof(LogRecord.SeverityText) == propertyName) {
            propertyValue = this._LogRecord.SeverityText;
            return true;
        }
        if (this._LogRecord.Attributes is { } attributes) {
            foreach (var attribute in attributes) {
                if (attribute.Key == propertyName) {
                    propertyValue = attribute.Value;
                    return true;
                }
            }
        }
        propertyValue = null;
        return false;
    }
}
