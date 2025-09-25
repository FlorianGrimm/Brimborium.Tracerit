namespace Brimborium.Tracerit.DataAccessor;

public sealed class LoggerTracorDataPool : ReferenceCountPool<LoggerTracorData> {
    public static LoggerTracorDataPool Create(IServiceProvider provider) => new(0);

    public LoggerTracorDataPool(int capacity) : base(capacity) {
    }
    protected override LoggerTracorData Create() => new LoggerTracorData(this);
}