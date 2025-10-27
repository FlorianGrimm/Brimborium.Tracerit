#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Brimborium.Tracerit.DataAccessor;

public sealed class LoggerTracorDataFactory
    : ITracorDataAccessorFactory<LoggerTracorData> {
    private readonly LoggerTracorDataPool? _LoggerTracorDataPool;

    public LoggerTracorDataFactory() {
    }

    public LoggerTracorDataFactory(LoggerTracorDataPool loggerTracorDataPool) {
        this._LoggerTracorDataPool = loggerTracorDataPool;
    }

    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is LoggerTracorData loggerTracorData) {
            tracorData = loggerTracorData;
            return true;
        }
        tracorData = null;
        return false;
    }

    public bool TryGetDataTyped(LoggerTracorData value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        tracorData = value;
        return true;
    }
}