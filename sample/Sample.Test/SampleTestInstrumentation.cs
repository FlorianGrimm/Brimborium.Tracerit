namespace Sample.WebApp;

[DisplayName(ActivitySourceName)]
public class SampleTestInstrumentation : Brimborium.Tracerit.ActivitySourceBase {
    public const string ActivitySourceName = "sample.test";
    public const string ActivitySourceVersion = "1.0.0";

    public SampleTestInstrumentation(
        Microsoft.Extensions.Configuration.IConfiguration? configuration
    ) : base(configuration, ActivitySourceName, ActivitySourceVersion) {
    }
}