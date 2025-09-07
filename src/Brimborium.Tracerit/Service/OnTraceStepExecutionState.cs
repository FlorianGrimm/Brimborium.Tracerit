namespace Brimborium.Tracerit.Service;

public sealed class OnTraceStepExecutionState {
    private readonly Dictionary<ValidatorStepIdentifier, ValidatorExpressionState> _DictStateByStep = new();

    public OnTraceStepExecutionState(TracorGlobalState? globalState = default, HashSet<string>? listSuccessState = default) {
        this.GlobalState = globalState ?? new TracorGlobalState();
        if (listSuccessState is { Count: > 0 }) {
            this.ListSuccessState = new HashSet<string>(listSuccessState);
        } else {
            this.ListSuccessState = new HashSet<string>();
        }
    }

    public TracorForkState ForkState { get; } = new();
    public TracorGlobalState GlobalState { get; }
    public HashSet<string> ListSuccessState { get; }

    public TState GetState<TState>(ValidatorStepIdentifier identifier)
        where TState : ValidatorExpressionState, new() {
        if (this._DictStateByStep.TryGetValue(identifier, out var state)) {
            return (TState)state;
        } else {
            var stateTyped = new TState();
            this._DictStateByStep[identifier] = stateTyped;
            return stateTyped;
        }
    }

    public bool TryGetState<TState>(ValidatorStepIdentifier identifier, [MaybeNullWhen(false)] out TState state)
        where TState : ValidatorExpressionState {
        if (this._DictStateByStep.TryGetValue(identifier, out var foundState)
            && foundState is TState foundStateTyped) {
            state = foundStateTyped;
            return true;
        }
        state = null;
        return false;
    }

    internal OnTraceStepExecutionState Copy() {
        var result = new OnTraceStepExecutionState(default, this.ListSuccessState);
        result.ForkState.CopyFrom(this.ForkState);
        result.GlobalState.CopyFrom(this.GlobalState);
        result._DictStateByStep.CopyFrom(this._DictStateByStep);
        return result;
    }
}