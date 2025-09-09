namespace Brimborium.Tracerit;

/// <summary>
/// Represents a validation expression that can process trace events and determine success or failure.
/// </summary>
public interface IValidatorExpression {
    /// <summary>
    /// Gets the unique instance index for this validator expression.
    /// </summary>
    /// <returns>The instance index.</returns>
    int GetInstanceIndex();

    /// <summary>
    /// Gets or sets the optional label for this validator expression.
    /// </summary>
    string? Label { get; }

    /// <summary>
    /// Processes a trace event and returns the result of the validation.
    /// </summary>
    /// <param name="callee">The identifier of the caller or trace point.</param>
    /// <param name="tracorData">The trace data to validate.</param>
    /// <param name="currentContext">The current context of the validation step.</param>
    /// <returns>The result of the trace validation.</returns>
    OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext);
}

/// <summary>
/// Represents the result of processing a trace event in a validator expression.
/// </summary>
public enum OnTraceResult {
    /// <summary>
    /// No specific result or the validation is still in progress.
    /// </summary>
    None,

    /// <summary>
    /// The validation was successful.
    /// </summary>
    Successfull
}
