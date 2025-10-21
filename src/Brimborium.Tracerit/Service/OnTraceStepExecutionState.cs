namespace Brimborium.Tracerit.Service;

public sealed class OnTraceStepExecutionState {
    internal readonly Dictionary<ValidatorStepIdentifier, ValidatorExpressionState> DictStateByStep = new();

    public OnTraceStepExecutionState(TracorGlobalState? globalState = default) {
        this.GlobalState = globalState ?? new TracorGlobalState();
    }

    public TracorForkState ForkState { get; } = new();
    public TracorGlobalState GlobalState { get; }

    public Dictionary<ValidatorStepIdentifier, TracorValidatorOnTraceResult> GetDictCompleted() {
        Dictionary<ValidatorStepIdentifier, TracorValidatorOnTraceResult> result = new();
        foreach (var kvp in this.DictStateByStep) {
            result[kvp.Key] = kvp.Value.Result;
        }
        return result;
    }

    public TState GetState<TState>(ValidatorStepIdentifier identifier)
        where TState : ValidatorExpressionState, new() {
        if (this.DictStateByStep.TryGetValue(identifier, out var state)) {
            return (TState)state;
        } else {
            var stateTyped = new TState();
            this.DictStateByStep[identifier] = stateTyped;
            return stateTyped;
        }
    }

    public bool TryGetState<TState>(ValidatorStepIdentifier identifier, [MaybeNullWhen(false)] out TState state)
        where TState : ValidatorExpressionState {
        if (this.DictStateByStep.TryGetValue(identifier, out var foundState)
            && foundState is TState foundStateTyped) {
            state = foundStateTyped;
            return true;
        }
        state = null;
        return false;
    }

    internal OnTraceStepExecutionState Copy() {
        var result = new OnTraceStepExecutionState(default);
        result.ForkState.CopyFrom(this.ForkState);
        result.GlobalState.CopyFrom(this.GlobalState);
        result.DictStateByStep.CopyFrom(this.DictStateByStep);
        return result;
    }
}