namespace Brimborium.Tracerit.Logger;

/// <summary>
/// Represents information about exceptions that is captured by TracorLogger
/// </summary>
[System.Diagnostics.Tracing.EventData(Name = "ExceptionInfo")]
internal sealed class ExceptionInfo {
    public static ExceptionInfo Empty { get; } = new ExceptionInfo();

    private ExceptionInfo() {
    }

    public ExceptionInfo(Exception exception) {
        this.TypeName = exception.GetType().FullName;
        this.Message = exception.Message;
        this.HResult = exception.HResult;
        this.VerboseMessage = exception.ToString();
    }

    public string? TypeName { get; }
    public string? Message { get; }
    public int HResult { get; }
    public string? VerboseMessage { get; } // This is the ToString() of the Exception
}