


namespace Brimborium.Tracerit.Service;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class OnTraceStepExecutionState {
#pragma warning disable IDE1006 // Naming Styles
    internal readonly Dictionary<ValidatorStepIdentifier, ValidatorExpressionState> DictStateByStep = new();
#pragma warning restore IDE1006 // Naming Styles

    public readonly Lock Lock = new Lock();
    private static long _IdNext = 0;
    public readonly long Id = System.Threading.Interlocked.Increment(ref _IdNext);

    public OnTraceStepExecutionState(List<TracorDataProperty>? globalState = default) {
        this.ListReportState = ImmutableArray<TracorReportStateItem>.Empty;
        this.DictForkState = ImmutableDictionary<string, TracorDataProperty>.Empty;
        if (globalState is null) {
            this.DictGlobalState = ImmutableDictionary<string, TracorDataProperty>.Empty;
        } else {
            this.DictGlobalState = globalState.ToImmutableDictionary(i => i.Name);
        }
    }

    private OnTraceStepExecutionState(OnTraceStepExecutionState src) {
        this.ListReportState = src.ListReportState;
        this.DictForkState = src.DictForkState;
        this.DictGlobalState = src.DictGlobalState;
        foreach (var kv in src.DictStateByStep) {
            this.DictStateByStep[kv.Key] = kv.Value.Copy();
        }
    }

    internal ImmutableArray<TracorReportStateItem> ListReportState;
    internal ImmutableDictionary<string, TracorDataProperty> DictForkState;
    internal ImmutableDictionary<string, TracorDataProperty> DictGlobalState;

    public TracorGlobalState GlobalState => new TracorGlobalState(this);

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
        return new OnTraceStepExecutionState(this);
    }

    internal TracorRunningState GetTracorRunningState()
        => new TracorRunningState(
            this.ListReportState,
            this.DictGlobalState);

    internal TracorFinishState GetFinishState(TracorValidatorOnTraceResult finalResult)
        => new TracorFinishState(
            finalResult,
            this.ListReportState,
            this.DictGlobalState);

    private string GetDebuggerDisplay()
        => $"{this.Id} SbS:{this.DictStateByStep.Count}; FS:{this.DictForkState.Count}; GS:{this.DictGlobalState.Count}";

    internal void SetValueForkState(TracorDataProperty propertyValue) {
        this.DictForkState = this.DictForkState.SetItem(propertyValue.Name, propertyValue);
    }

    internal void AddReportState(TracorReportStateItem value) {
        this.ListReportState = this.ListReportState.Add(value);
    }

    internal void SetValueGlobalState(TracorDataProperty value) {
        this.DictGlobalState=this.DictGlobalState.SetItem(value.Name, value);
    }
}
