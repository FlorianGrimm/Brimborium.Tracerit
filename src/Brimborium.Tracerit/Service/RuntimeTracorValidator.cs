namespace Brimborium.Tracerit.Service;

internal sealed partial class RuntimeTracorValidator : ITracorValidator {
    private readonly ILogger<RuntimeTracorValidator> _Logger;

    public RuntimeTracorValidator(
        ILogger<RuntimeTracorValidator> logger
        ) {
        this._Logger = logger;
    }
    public ITracorValidatorPath Add(IValidatorExpression step, TracorGlobalState? globalState = default) {
        return new RuntimeTracorValidatorPath(step);
    }

    [Microsoft.Extensions.Logging.LoggerMessage(1, LogLevel.Debug, "RuntimeTracorValidator.OnTrace - Should not be called {callee}")]
    partial void OnTraceLog(TracorIdentitfier callee);

    public void OnTrace(TracorIdentitfier callee, ITracorData tracorData) {
        this.OnTraceLog(callee);
    }
}

internal sealed class RuntimeTracorValidatorPath : ITracorValidatorPath {
    private readonly IValidatorExpression _Step;

    public RuntimeTracorValidatorPath(IValidatorExpression step) {
        this._Step = step;
    }

    public void OnTrace(TracorIdentitfier callee, ITracorData tracorData) {
    }

    public TracorGlobalState? GetRunnging(string searchSuccessState) => default;
    public Task<TracorGlobalState?> GetRunngingAsync(string searchSuccessState, TimeSpan timeout = default) => Task.FromResult(default(TracorGlobalState));

    public TracorGlobalState? GetFinished(Predicate<TracorGlobalState>? predicate = default) => default;
    public Task<TracorGlobalState?> GetFinishedAsync(Predicate<TracorGlobalState>? predicate = default, TimeSpan timeSpan = default) => Task.FromResult(default(TracorGlobalState));

    public List<TracorGlobalState> GetListRunnging() => [];
    public List<TracorGlobalState> GetListFinished() => [];

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    void IDisposable.Dispose() { }
}