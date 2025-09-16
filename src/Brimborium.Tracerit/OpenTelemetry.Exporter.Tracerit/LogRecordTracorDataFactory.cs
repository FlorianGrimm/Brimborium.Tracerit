using OpenTelemetry.Logs;

namespace OpenTelemetry.Exporter.Tracerit;

public sealed class LogRecordTracorDataFactory
    : ITracorDataAccessorFactory<LogRecord> {
    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is LogRecord logRecord) {
            tracorData = new LogRecordTracorData(logRecord);
            return true;
        }
        tracorData = default;
        return false;
    }

    public bool TryGetDataTyped(LogRecord value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        tracorData = new LogRecordTracorData(value);
        return true;
    }
}
