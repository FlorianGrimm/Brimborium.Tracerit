namespace Brimborium.Tracerit;

public interface ITracorValidator {
    ITracorValidatorPath Add(IValidatorExpression step, TracorGlobalState? globalState=default);

    void OnTrace(TracorIdentitfier callee, ITracorData tracorData);
}

public interface ITracorValidatorPath : IDisposable {
    void OnTrace(TracorIdentitfier callee, ITracorData tracorData);

    TracorGlobalState? GetFinished(Predicate<TracorGlobalState>? predicate);
    Task<TracorGlobalState?> GetFinishedAsync(Predicate<TracorGlobalState>? predicate, TimeSpan timeSpan=default);
    
    List<TracorGlobalState> GetListRunnging();
    List<TracorGlobalState> GetListFinished();
    Task<TracorGlobalState?> GetRunngingAsync(string searchSuccessState, TimeSpan timeout = default);
    TracorGlobalState? GetRunnging(string searchSuccessState);
}
