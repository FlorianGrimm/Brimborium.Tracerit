namespace Brimborium.Tracerit.Service;

internal sealed class NeverTracorValidatorPath : ITracorValidatorPath {
    public void OnTrace(TracorIdentitfier callee, ITracorData tracorData) { }

    public TracorGlobalState? GetRunnging(string searchSuccessState) => default;
    public Task<TracorGlobalState?> GetRunngingAsync(string searchSuccessState, TimeSpan timeout = default) => Task.FromResult(default(TracorGlobalState));

    public TracorGlobalState? GetFinished(Predicate<TracorGlobalState>? predicate = default) => default;
    public Task<TracorGlobalState?> GetFinishedAsync(Predicate<TracorGlobalState>? predicate = default, TimeSpan timeSpan = default) => Task.FromResult(default(TracorGlobalState));

    public List<TracorGlobalState> GetListRunnging() => [];
    public List<TracorGlobalState> GetListFinished() => [];

    public void Dispose() { }


}