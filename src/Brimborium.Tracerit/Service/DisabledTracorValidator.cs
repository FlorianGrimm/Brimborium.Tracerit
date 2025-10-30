namespace Brimborium.Tracerit.Service;

internal sealed partial class DisabledTracorValidator : ITracorValidator {
    private readonly ILogger<DisabledTracorValidator> _Logger;

    public DisabledTracorValidator(
        ILogger<DisabledTracorValidator> logger
        ) {
        this._Logger = logger;
    }

    public ITracorValidatorPath? GetExisting(IValidatorExpression step) => default;

    public ITracorValidatorPath Add(IValidatorExpression step, List<TracorDataProperty>? globalState = default) {
        return new DisabledTracorValidatorPath(step);
    }
    public bool IsGeneralEnabled() => false;

    public bool IsEnabled() => false;

    [Microsoft.Extensions.Logging.LoggerMessage(1, LogLevel.Debug, "RuntimeTracorValidator.OnTrace - Should not be called {callee}")]
    partial void OnTraceLog(TracorIdentifier callee);

    public void OnTrace(bool isPublic, ITracorData tracorData) {
        this.OnTraceLog(tracorData.TracorIdentifier);
    }

}

internal sealed class DisabledTracorValidatorPath : ITracorValidatorPath {
    private readonly IValidatorExpression _Step;

    public DisabledTracorValidatorPath(IValidatorExpression step) {
        this._Step = step;
    }

    public IValidatorExpression Step => this._Step;

    public bool EnableFinished { get; set; }

    public void OnTrace(ITracorData tracorData) { }

    public TracorRunningState? GetRunning(string searchSuccessState) => default;
    public Task<TracorRunningState?> GetRunningAsync(string searchSuccessState, TimeSpan timeout = default) => Task.FromResult(default(TracorRunningState));

    public TracorFinishState? GetFinished(Predicate<TracorFinishState>? predicate = default) => default;
    public Task<TracorFinishState?> GetFinishedAsync(Predicate<TracorFinishState>? predicate = default, TimeSpan timeSpan = default) => Task.FromResult(default(TracorFinishState));

    public List<TracorRunningState> GetListRunning() => [];
    public List<TracorFinishState> GetListFinished() => [];

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    void IDisposable.Dispose() { }

    public IDisposable AddFinishCallback(Action<ITracorValidatorPath, TracorFinishState> callback)
        => new DisabledDisposable();

    class DisabledDisposable : IDisposable {
        public void Dispose() { }
    }
}