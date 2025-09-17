namespace Sample.WebApp;

public class SampleInstrumentation {
    internal const string ActivitySourceName = "sample";
    internal const string ActivitySourceVersion = "1.0.0";

    public SampleInstrumentation() {
    }

    public static ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName, ActivitySourceVersion);
}