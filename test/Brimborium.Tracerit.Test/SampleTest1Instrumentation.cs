namespace Brimborium.Tracerit.Test;

[DisplayName(ActivitySourceName)]
public class SampleTest1Instrumentation : Brimborium.Tracerit.ActivitySourceBase {
    public const string ActivitySourceName = "sample.test1";
    public const string ActivitySourceVersion = "1.0.0";

    public SampleTest1Instrumentation(
        IConfiguration? configuration
    ) : base(configuration, ActivitySourceName, ActivitySourceVersion) {
    }
}

[DisplayName(ActivitySourceName)]
public class SampleTest2Instrumentation : Brimborium.Tracerit.ActivitySourceBase {
    public const string ActivitySourceName = "sample.test2";
    public const string ActivitySourceVersion = "1.0.0";

    public SampleTest2Instrumentation(
        IConfiguration? configuration
    ) : base(configuration, ActivitySourceName, ActivitySourceVersion) {
    }
}


[DisplayName(ActivitySourceName)]
public class SampleTest3Instrumentation : Brimborium.Tracerit.ActivitySourceBase {
    public const string ActivitySourceName = "sample.test3";
    public const string ActivitySourceVersion = "1.0.0";

    public SampleTest3Instrumentation(
        IConfiguration? configuration
    ) : base(configuration, ActivitySourceName, ActivitySourceVersion) {
    }
}