namespace Brimborium.Tracerit.Service;

internal sealed class NeverTracorValidatorPath : ITracorValidatorPath {
    public IValidatorExpression Step => throw new NotSupportedException();

    public bool EnableFinished { get ; set ; }

    public void OnTrace(ITracorData tracorData) { }

    public TracorGlobalState? GetRunning(string searchSuccessState) => default;
    public Task<TracorGlobalState?> GetRunningAsync(string searchSuccessState, TimeSpan timeout = default) => Task.FromResult(default(TracorGlobalState));

    public TracorGlobalState? GetFinished(Predicate<TracorGlobalState>? predicate = default) => default;
    public Task<TracorGlobalState?> GetFinishedAsync(Predicate<TracorGlobalState>? predicate = default, TimeSpan timeSpan = default) => Task.FromResult(default(TracorGlobalState));

    public List<TracorGlobalState> GetListRunning() => [];
    public List<TracorGlobalState> GetListFinished() => [];

    public void Dispose() { }

    public IDisposable AddFinishCallback(Action<ITracorValidatorPath, OnTraceStepExecutionState> callback) {
        return new DisabledDisposable();
    }
    class DisabledDisposable : IDisposable {
        public void Dispose() { }
    }
}