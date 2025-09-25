using Brimborium.Tracerit.Diagnostics;

namespace Sample.WebApp;

public class SampleTestInstrumentation : InstrumentationBase {
    public const string ActivitySourceName = "sample.test";
    public const string ActivitySourceVersion = "1.0.0";

    public SampleTestInstrumentation()
        : base(ActivitySourceName, ActivitySourceVersion) {
    }
}