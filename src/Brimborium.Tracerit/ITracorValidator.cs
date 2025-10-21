namespace Brimborium.Tracerit;

/// <summary>
/// Validates trace data against defined expressions and manages validation paths.
/// </summary>
public interface ITracorValidator : ITracorCollectiveSink {
    ITracorValidatorPath? GetExisting(IValidatorExpression step);

    /// <summary>
    /// Adds a validation expression with optional global state and returns a validation path.
    /// </summary>
    /// <param name="step">The validator expression to add.</param>
    /// <param name="globalState">Optional global state to initialize the validation with.</param>
    /// <returns>A validation path that can be used to track the validation progress.</returns>
    ITracorValidatorPath Add(IValidatorExpression step, TracorGlobalState? globalState = default);
}

/// <summary>
/// Represents a validation path that tracks the progress of validation expressions.
/// </summary>
public interface ITracorValidatorPath : IDisposable {
    IValidatorExpression Step { get; }

    bool EnableFinished { get; set; }

    IDisposable AddFinishCallback(
       Action<ITracorValidatorPath, OnTraceStepExecutionState> callback
       );

    /// <summary>
    /// Processes a trace event for this validation path.
    /// </summary>
    /// <param name="tracorData">The trace data to process.</param>
    void OnTrace(ITracorData tracorData);

    /// <summary>
    /// Gets a finished validation state that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">Optional predicate to filter the finished states.</param>
    /// <returns>A finished validation state if found; otherwise, null.</returns>
    TracorGlobalState? GetFinished(Predicate<TracorGlobalState>? predicate = default);

    /// <summary>
    /// Asynchronously gets a finished validation state that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">Optional predicate to filter the finished states.</param>
    /// <param name="timeSpan">The maximum time to wait for a finished state.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a finished validation state if found; otherwise, null.</returns>
    Task<TracorGlobalState?> GetFinishedAsync(Predicate<TracorGlobalState>? predicate = default, TimeSpan timeSpan = default);

    /// <summary>
    /// Gets a list of all currently running validation states.
    /// </summary>
    /// <returns>A list of running validation states.</returns>
    List<TracorGlobalState> GetListRunning();

    /// <summary>
    /// Gets a list of all finished validation states.
    /// </summary>
    /// <returns>A list of finished validation states.</returns>
    List<TracorGlobalState> GetListFinished();

    /// <summary>
    /// Asynchronously gets a running validation state that matches the specified success state.
    /// </summary>
    /// <param name="searchSuccessState">The success state to search for.</param>
    /// <param name="timeout">The maximum time to wait for the running state.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a running validation state if found; otherwise, null.</returns>
    Task<TracorGlobalState?> GetRunningAsync(string searchSuccessState, TimeSpan timeout = default);

    /// <summary>
    /// Gets a running validation state that matches the specified success state.
    /// </summary>
    /// <param name="searchSuccessState">The success state to search for.</param>
    /// <returns>A running validation state if found; otherwise, null.</returns>
    TracorGlobalState? GetRunning(string searchSuccessState);
}
