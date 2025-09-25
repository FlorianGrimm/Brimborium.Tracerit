using Brimborium.Tracerit.Diagnostics;

namespace Brimborium.Tracerit.Test;

public sealed class SampleTestInstrumentation : InstrumentationBase {
    public const string ActivitySourceName = "sample.test";
    public SampleTestInstrumentation() :base(ActivitySourceName) { }
}

public sealed class SampleTest1Instrumentation : InstrumentationBase {
    public SampleTest1Instrumentation() { }
}

public sealed class SampleTest2Instrumentation : InstrumentationBase {
    public const string ActivitySourceName = "sample.test2";
    public const string ActivitySourceVersion = "1.0.0";

    public SampleTest2Instrumentation() : base(ActivitySourceName, ActivitySourceVersion) { }
}


[DisplayName(ActivitySourceName)]
public sealed class SampleTest3Instrumentation : InstrumentationBase {
    public const string ActivitySourceName = "sample.test3";
    public SampleTest3Instrumentation() { }
}