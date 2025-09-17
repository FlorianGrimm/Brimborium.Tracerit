using System.Diagnostics;

namespace Sample.WebApp;

public class SampleTestInstrumentation {
    public const string ActivitySourceName = "sample.test";
    public const string ActivitySourceVersion = "1.0.0";

    public SampleTestInstrumentation() {
    }

    public static ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName, ActivitySourceVersion);
}