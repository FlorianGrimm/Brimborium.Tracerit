namespace Brimborium.Tracerit.Service;

public struct OnTraceStepCurrentContext {
    private readonly ValidatorStepIdentifier _Identifier;
    private readonly OnTraceStepExecutionState _ExecutionState;
    private readonly TracorValidatorPathModifications _Modifications;
    private readonly LoggerUtility _LoggerUtility;

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

    public ValidatorStepIdentifier Identifier => this._Identifier;

    public readonly TracorForkState ForkState => this._ExecutionState.ForkState;
    public readonly TracorGlobalState GlobalState => this._ExecutionState.GlobalState;

    public LoggerUtility LoggerUtility => this._LoggerUtility;

    public TState GetState<TState>()
        where TState : ValidatorExpressionState, new() {
        return this._ExecutionState.GetState<TState>(this.Identifier);
    }

    public TState GetState<TState>(int childIndex)
        where TState : ValidatorExpressionState, new() {
        return this._ExecutionState.GetState<TState>(this.Identifier.GetChildIdentifier(childIndex));
    }

    public readonly bool TryGetState<TState>([MaybeNullWhen(false)] out TState state)
        where TState : ValidatorExpressionState {
        return this._ExecutionState.TryGetState(this._Identifier, out state);
    }

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

    public readonly void CreateFork(string propertyName, object propertyValue, IEqualityComparer equalityComparer) {
        var copy = this._ExecutionState.Copy();
        this._ExecutionState.GlobalState[propertyName] = propertyValue;
        this._ExecutionState.ForkState.SetItem(propertyName, propertyValue, equalityComparer);
        this._Modifications.AddFork(this._ExecutionState, copy);
    }

    public readonly OnTraceStepExecutionState? TryGetFork(string propertyName, object propertyValue) {
        TracorForkState forkState = new(this._ExecutionState.ForkState);
        forkState[propertyName] = propertyValue;
        return this._Modifications.TryGetFork(forkState);
    }
    public readonly void SetStateSuccessfull(IValidatorExpression validatorExpression, ValidatorExpressionState state) {
        state.Successfull = true;
        if (validatorExpression.Label is { } label) {
            this._ExecutionState.ListSuccessState.Add(label);
        }
    }

    // public void SuccessFull() {     }
}
