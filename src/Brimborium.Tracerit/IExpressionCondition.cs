namespace Brimborium.Tracerit;

/// <summary>
/// Represents a condition that can be evaluated against trace data to determine if it matches specific criteria.
/// </summary>
public interface IExpressionCondition {
    /// <summary>
    /// Determines whether the specified trace data matches this condition.
    /// </summary>
    /// <param name="callee">The identifier of the caller or trace point.</param>
    /// <param name="tracorData">The trace data to evaluate.</param>
    /// <param name="currentContext">The current context of the validation step.</param>
    /// <returns>True if the trace data matches the condition; otherwise, false.</returns>
    bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext);
}

/// <summary>
/// Represents a strongly-typed condition that can be evaluated against trace data.
/// </summary>
/// <typeparam name="T">The type of data this condition can evaluate.</typeparam>
public interface IExpressionCondition<T>: IExpressionCondition {
}