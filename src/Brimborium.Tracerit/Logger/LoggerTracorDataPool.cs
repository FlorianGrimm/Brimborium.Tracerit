using Brimborium.Tracerit.Utility;

namespace Brimborium.Tracerit.Logger;

public sealed class LoggerTracorDataPool : ReferenceCountPool<LoggerTracorData> {
    public LoggerTracorDataPool(int capacity) : base(capacity) {
    }

    protected override LoggerTracorData Create() {
        return new LoggerTracorData(this);
    }
}