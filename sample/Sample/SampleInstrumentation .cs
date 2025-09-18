namespace Sample.WebApp;

public class SampleInstrumentation : Brimborium.Tracerit.ActivitySourceBase {
    internal const string ActivitySourceName = "sample";
    internal const string ActivitySourceVersion = "1.0.0";

    public SampleInstrumentation(
        IConfiguration? configuration        
    ) : base(configuration, ActivitySourceName, ActivitySourceVersion) {
    }
}