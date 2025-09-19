namespace Brimborium.Tracerit.Test;

[DisplayName(ActivitySourceName)]
public sealed class SampleTest1Instrumentation : Brimborium.Tracerit.ActivitySourceBase {
    public const string ActivitySourceName = "sample.test1";
    public const string ActivitySourceVersion = "1.0.0";

    public static SampleTest1Instrumentation GetInstance() => ActivitySourceBase.GetInstanceByType<SampleTest1Instrumentation>();

    public SampleTest1Instrumentation() : base(default, ActivitySourceName, ActivitySourceVersion) { }
    public SampleTest1Instrumentation(IConfiguration? configuration) : base(configuration, ActivitySourceName, ActivitySourceVersion) {
    }
}

[DisplayName(ActivitySourceName)]
public sealed class SampleTest2Instrumentation : Brimborium.Tracerit.ActivitySourceBase {
    public const string ActivitySourceName = "sample.test2";
    public const string ActivitySourceVersion = "1.0.0";

    public static SampleTest2Instrumentation GetInstance() => ActivitySourceBase.GetInstanceByType<SampleTest2Instrumentation>();

    public SampleTest2Instrumentation() : base(default, ActivitySourceName, ActivitySourceVersion) { }
    public SampleTest2Instrumentation(IConfiguration? configuration) : base(configuration, ActivitySourceName, ActivitySourceVersion) { }
}


[DisplayName(ActivitySourceName)]
public sealed class SampleTest3Instrumentation : Brimborium.Tracerit.ActivitySourceBase {
    public const string ActivitySourceName = "sample.test3";
    public const string ActivitySourceVersion = "1.0.0";

    public static SampleTest3Instrumentation GetInstance() => ActivitySourceBase.GetInstanceByType<SampleTest3Instrumentation>();

    public SampleTest3Instrumentation() : base(default, ActivitySourceName, ActivitySourceVersion) { }
    public SampleTest3Instrumentation(IConfiguration? configuration) : base(configuration, ActivitySourceName, ActivitySourceVersion) { }
}