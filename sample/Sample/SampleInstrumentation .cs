using Brimborium.Tracerit.Diagnostics;

namespace Sample.WebApp;

public class SampleInstrumentation : InstrumentationBase {
    internal const string ActivitySourceName = "sample";
    internal const string ActivitySourceVersion = "1.0.0";

    public SampleInstrumentation()
        : base(ActivitySourceName, ActivitySourceVersion) {
    }
}