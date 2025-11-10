namespace ReadAndWrite;

public sealed class ReadAndWriteInstrumentation : InstrumentationBase {
    internal const string ActivitySourceName = "ReadAndWrite";
    internal const string ActivitySourceVersion = "1.0.0";

    public ReadAndWriteInstrumentation()
        : base(ActivitySourceName, ActivitySourceVersion) {
    }
}