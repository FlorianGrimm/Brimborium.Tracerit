namespace Brimborium.Tracerit;

public sealed class TraceritInstrumentation : InstrumentationBase {
}

public sealed class TraceritBreakLoopInstrumentation : InstrumentationBase {
    public const string ActivitySourceName = "TraceritBreakLoop";
    
    private static TraceritBreakLoopInstrumentation? _Instance;
    public static TraceritBreakLoopInstrumentation Instance
        => _Instance ?? new();

    public TraceritBreakLoopInstrumentation() 
        : base(ActivitySourceName) {
    }
}
