namespace Brimborium.Tracerit.Service;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class OnTraceStepExecutionState {
#pragma warning disable IDE1006 // Naming Styles
    internal readonly Dictionary<ValidatorStepIdentifier, ValidatorExpressionState> DictStateByStep = new();
#pragma warning restore IDE1006 // Naming Styles

    public readonly Lock Lock = new Lock();
    private static long _IdNext = 0;
    public readonly long Id = System.Threading.Interlocked.Increment(ref _IdNext);

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
        {
            foreach (var kv in this.ForkState) {
                result.ForkState[kv.Key] = kv.Value;
            }
        }
        {
            foreach (var kv in this.GlobalState) {
                result.GlobalState[kv.Key] = kv.Value;
            }
        }
        {
            foreach (var kv in this.DictStateByStep) { 
                result.DictStateByStep[kv.Key]= kv.Value.Copy();
            }
        }
        return result;
    }

    internal TracorRunningState GetTracorRunningState()
        => new TracorRunningState(
            ForkState: new(this.ForkState),
            GlobalState: new(this.GlobalState));

    internal TracorFinishState GetFinishState(TracorValidatorOnTraceResult finalResult)
        => new TracorFinishState(
            finalResult,
            this.ForkState,
            this.GlobalState);

    private string GetDebuggerDisplay()
        => $"{this.Id} SbS:{this.DictStateByStep.Count}; FS:{this.ForkState.Count}; GS:{this.GlobalState.Count}";
}
