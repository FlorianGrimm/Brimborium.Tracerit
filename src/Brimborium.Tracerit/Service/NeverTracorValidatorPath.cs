namespace Brimborium.Tracerit.Service;

internal sealed class NeverTracorValidatorPath : ITracorValidatorPath {
    public IValidatorExpression Step => throw new NotSupportedException();

    public bool EnableFinished { get; set; }

    public void OnTrace(ITracorData tracorData) { }

    public TracorRunningState? GetRunning(string searchSuccessState) => default;
    public Task<TracorRunningState?> GetRunningAsync(string searchSuccessState, TimeSpan timeout = default) => Task.FromResult(default(TracorRunningState));

    public TracorFinishState? GetFinished(Predicate<TracorFinishState>? predicate = default) => default;
    public Task<TracorFinishState?> GetFinishedAsync(Predicate<TracorFinishState>? predicate = default, TimeSpan timeSpan = default) => Task.FromResult(default(TracorFinishState));

    public List<TracorRunningState> GetListRunning() => [];
    public List<TracorFinishState> GetListFinished() => [];

    public void Dispose() { }

    public IDisposable AddFinishCallback(Action<ITracorValidatorPath, TracorFinishState> callback)
        => new DisabledDisposable();

    class DisabledDisposable : IDisposable {
        public void Dispose() { }
    }
}