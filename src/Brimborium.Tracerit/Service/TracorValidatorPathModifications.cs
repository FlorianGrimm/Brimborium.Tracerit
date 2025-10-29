namespace Brimborium.Tracerit.Service;

public sealed class TracorValidatorPathModifications {
    private readonly TracorValidatorPath _TracorValidatorPath;

    internal TracorValidatorPathModifications(TracorValidatorPath tracorValidatorPath) {
        this._TracorValidatorPath = tracorValidatorPath;
    }

    internal void AddFork(OnTraceStepExecutionState before, OnTraceStepExecutionState value) {
        this._TracorValidatorPath.AddFork(before, value);
    }

    internal OnTraceStepExecutionState? TryGetFork(in TracorForkState forkState)
        => this._TracorValidatorPath.TryGetFork(forkState);

    internal OnTraceStepExecutionState? TryGetFork(in TracorDataProperty tdpCurrent, Func<TracorDataProperty, TracorDataProperty, bool> fnCompare)
        => this._TracorValidatorPath.TryGetFork(tdpCurrent, fnCompare);
}
