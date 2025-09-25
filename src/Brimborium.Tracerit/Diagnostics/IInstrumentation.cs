namespace Brimborium.Tracerit.Diagnostics;

public interface IInstrumentation : IDisposable {
    ActivitySource? ActivitySource { get; }
}
