namespace Brimborium.Tracerit.Service;

/// <summary>
/// Represents the current context for a validation step, providing access to state, identifiers, and utilities.
/// This struct encapsulates all the information needed during the execution of a validation step.
/// </summary>
public readonly struct OnTraceStepCurrentContext {
    private readonly ValidatorStepIdentifier _Identifier;
    private readonly OnTraceStepExecutionState _ExecutionState;
    private readonly TracorValidatorPathModifications _Modifications;
    private readonly LoggerUtility _LoggerUtility;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnTraceStepCurrentContext"/> struct.
    /// </summary>
    /// <param name="identifier">The identifier for the current validation step.</param>
    /// <param name="executionState">The execution state containing global and fork state information.</param>
    /// <param name="modifications">The modifications tracker for managing forks and state changes.</param>
    /// <param name="loggerUtility">The logger utility for logging operations.</param>
    public OnTraceStepCurrentContext(
        ValidatorStepIdentifier identifier,
        OnTraceStepExecutionState executionState,
        TracorValidatorPathModifications modifications,
        LoggerUtility loggerUtility) {
        this._Identifier = identifier;
        this._ExecutionState = executionState;
        this._Modifications = modifications;
        this._LoggerUtility = loggerUtility;
    }

    /// <summary>
    /// Gets the identifier for the current validation step.
    /// </summary>
    public ValidatorStepIdentifier Identifier => this._Identifier;

    /// <summary>
    /// Gets the fork state for the current execution context.
    /// </summary>
    public readonly TracorForkState ForkState => this._ExecutionState.ForkState;

    /// <summary>
    /// Gets the global state for the current execution context.
    /// </summary>
    public readonly TracorGlobalState GlobalState => this._ExecutionState.GlobalState;

    /// <summary>
    /// Gets the logger utility for logging operations.
    /// </summary>
    public LoggerUtility LoggerUtility => this._LoggerUtility;

    /// <summary>
    /// Gets or creates a state object for the current validation step.
    /// </summary>
    /// <typeparam name="TState">The type of state to retrieve or create.</typeparam>
    /// <returns>The state object for the current step.</returns>
    public TState GetState<TState>()
        where TState : ValidatorExpressionState, new() {
        return this._ExecutionState.GetState<TState>(this.Identifier);
    }

    /// <summary>
    /// Gets or creates a state object for a child validation step.
    /// </summary>
    /// <typeparam name="TState">The type of state to retrieve or create.</typeparam>
    /// <param name="childIndex">The index of the child step.</param>
    /// <returns>The state object for the specified child step.</returns>
    public TState GetState<TState>(int childIndex)
        where TState : ValidatorExpressionState, new() {
        return this._ExecutionState.GetState<TState>(this.Identifier.GetChildIdentifier(childIndex));
    }

    /// <summary>
    /// Attempts to get an existing state object for the current validation step.
    /// </summary>
    /// <typeparam name="TState">The type of state to retrieve.</typeparam>
    /// <param name="state">When this method returns, contains the state object if found; otherwise, null.</param>
    /// <returns>True if the state was found; otherwise, false.</returns>
    public readonly bool TryGetState<TState>([MaybeNullWhen(false)] out TState state)
        where TState : ValidatorExpressionState {
        return this._ExecutionState.TryGetState(this._Identifier, out state);
    }

    /// <summary>
    /// Attempts to get an existing state object for a child validation step.
    /// </summary>
    /// <typeparam name="TState">The type of state to retrieve.</typeparam>
    /// <param name="childIndex">The index of the child step.</param>
    /// <param name="state">When this method returns, contains the state object if found; otherwise, null.</param>
    /// <returns>True if the state was found; otherwise, false.</returns>
    public readonly bool TryGetState<TState>(int childIndex, [MaybeNullWhen(false)] out TState state)
        where TState : ValidatorExpressionState {
        return this._ExecutionState.TryGetState(this._Identifier.GetChildIdentifier(childIndex), out state);
    }

    public readonly OnTraceStepCurrentContext GetChildContext(int childIndex) {
        return new OnTraceStepCurrentContext(
            this._Identifier.GetChildIdentifier(childIndex),
            this._ExecutionState,
            this._Modifications,
            this._LoggerUtility);
    }

    public readonly void CreateFork(string propertyName, TracorDataProperty propertyValue) {
        var copy = this._ExecutionState.Copy();
        this._ExecutionState.GlobalState[propertyName] = propertyValue;
        this._ExecutionState.ForkState.SetItem(propertyName, propertyValue);
        this._Modifications.AddFork(this._ExecutionState, copy);
    }

    public readonly OnTraceStepExecutionState? TryGetFork(string propertyName, TracorDataProperty propertyValue) {
        TracorForkState forkState = new(this._ExecutionState.ForkState);
        forkState[propertyName] = propertyValue;
        return this._Modifications.TryGetFork(forkState);
    }

    public readonly OnTraceStepExecutionState? TryGetFork(string propertyName, TracorDataProperty tdpCurrent, Func<TracorDataProperty, TracorDataProperty, bool> fnCompare) {
        return this._Modifications.TryGetFork(propertyName, tdpCurrent, fnCompare);
    }

    public readonly void SetStateSuccessful(IValidatorExpression validatorExpression, ValidatorExpressionState state) {
        state.Result = TracorValidatorOnTraceResult.Successful;
    }

    public readonly void SetStateFailed(IValidatorExpression validatorExpression, ValidatorExpressionState state) {
        state.Result = TracorValidatorOnTraceResult.Failed;
    }

    public readonly TracorValidatorOnTraceResult SetStateComplete(
        IValidatorExpression validatorExpression, 
        ValidatorExpressionState state, 
        TracorValidatorOnTraceResult result) {
        state.Result = result;
        return result;
    }
}
