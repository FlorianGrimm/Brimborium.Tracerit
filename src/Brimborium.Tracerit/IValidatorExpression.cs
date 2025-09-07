namespace Brimborium.Tracerit;

public interface IValidatorExpression {
    int GetInstanceIndex();

    string? Label { get; }

    OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext);
}

public enum OnTraceResult { 
    None,
    Successfull
}
