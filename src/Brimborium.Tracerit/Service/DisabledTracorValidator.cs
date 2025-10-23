namespace Brimborium.Tracerit.Service;

internal sealed partial class DisabledTracorValidator : ITracorValidator {
    private readonly ILogger<DisabledTracorValidator> _Logger;

    public DisabledTracorValidator(
        ILogger<DisabledTracorValidator> logger
        ) {
        this._Logger = logger;
    }

    public ITracorValidatorPath? GetExisting(IValidatorExpression step) => default;

    public ITracorValidatorPath Add(IValidatorExpression step, TracorGlobalState? globalState = default) {
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

    public TracorGlobalState? GetRunning(string searchSuccessState) => default;
    public Task<TracorGlobalState?> GetRunningAsync(string searchSuccessState, TimeSpan timeout = default) => Task.FromResult(default(TracorGlobalState));

    public TracorGlobalState? GetFinished(Predicate<TracorGlobalState>? predicate = default) => default;
    public Task<TracorGlobalState?> GetFinishedAsync(Predicate<TracorGlobalState>? predicate = default, TimeSpan timeSpan = default) => Task.FromResult(default(TracorGlobalState));

    public List<TracorGlobalState> GetListRunning() => [];
    public List<TracorGlobalState> GetListFinished() => [];

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    void IDisposable.Dispose() { }

    public IDisposable AddFinishCallback(Action<ITracorValidatorPath, OnTraceStepExecutionState> callback) {
        return new DisabledDisposable();
    }
    class DisabledDisposable : IDisposable {
        public void Dispose() { }
    }
}