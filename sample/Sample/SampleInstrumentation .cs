namespace Sample.WebApp;

public class SampleInstrumentation : IDisposable
{
    internal const string ActivitySourceName = "sample";
    internal const string ActivitySourceVersion = "1.0.0";

    public SampleInstrumentation()
    {
        this.ActivitySource = new ActivitySource(ActivitySourceName, ActivitySourceVersion);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose()
    {
        this.ActivitySource.Dispose();
    }
}