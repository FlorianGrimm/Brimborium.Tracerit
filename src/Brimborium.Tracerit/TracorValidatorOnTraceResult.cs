namespace Brimborium.Tracerit;

/// <summary>
/// Represents the result of processing a trace event in a validator expression.
/// </summary>
public enum TracorValidatorOnTraceResult {
    /// <summary>
    /// No specific result or the validation is still in progress.
    /// </summary>
    None,

    /// <summary>
    /// The validation was successful.
    /// </summary>
    Successfull,

    /// <summary>
    /// The validation failed.
    /// </summary>
    Failed,
}

public static class TracorValidatorOnTraceResultExtension {
    public static bool IsComplete(this TracorValidatorOnTraceResult that)
        => that is TracorValidatorOnTraceResult.Successfull or TracorValidatorOnTraceResult.Failed;
}