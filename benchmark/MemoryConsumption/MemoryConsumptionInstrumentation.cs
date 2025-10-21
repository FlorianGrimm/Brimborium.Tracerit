namespace MemoryConsumption;

public sealed class MemoryConsumptionInstrumentation : InstrumentationBase {
    internal const string ActivitySourceName = "MemoryConsumption";
    internal const string ActivitySourceVersion = "1.0.0";

    public MemoryConsumptionInstrumentation()
        : base(ActivitySourceName, ActivitySourceVersion) {
    }
}