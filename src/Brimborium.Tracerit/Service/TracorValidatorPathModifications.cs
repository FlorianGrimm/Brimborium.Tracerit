namespace Brimborium.Tracerit.Service;

public sealed class TracorValidatorPathModifications {
    private TracorValidatorPath _TracorValidatorPath;

    internal TracorValidatorPathModifications(TracorValidatorPath tracorValidatorPath) {
        this._TracorValidatorPath = tracorValidatorPath;
    }

    internal void AddFork(OnTraceStepExecutionState before, OnTraceStepExecutionState value) {
        this._TracorValidatorPath.AddFork(before, value);
    }

    internal OnTraceStepExecutionState? TryGetFork(TracorForkState forkState)
        => this._TracorValidatorPath.TryGetFork(forkState);
}
