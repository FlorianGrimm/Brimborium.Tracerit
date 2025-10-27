namespace Brimborium.Tracerit.Service;

internal sealed class TracorValidatorPath : ITracorValidatorPath {
    private readonly Lock _Lock = new();
    private readonly IValidatorExpression _Step;
    private readonly IDisposable _OnDispose;
    private readonly LoggerUtility _LoggerUtility;
    private readonly TracorValidatorPathModifications _Modifications;
    private ImmutableArray<OnTraceStepExecutionState> _ListRunningExecutionState;
    private readonly List<OnTraceStepExecutionState> _ListFinishedExecutionState = new();
    private TaskCompletionSource<TracorValidatorPath> _TcsFinishedExecutionState = new();
    private ImmutableArray<CallbackDisposable> _ListFinishCallback = ImmutableArray<CallbackDisposable>.Empty;

    public TracorValidatorPath(
        IValidatorExpression step,
        TracorGlobalState? globalState,
        IDisposable onDispose,
        LoggerUtility loggerUtility) {
        this._Step = step;
        this._OnDispose = onDispose;
        this._LoggerUtility = loggerUtility;
        this._ListRunningExecutionState = [new OnTraceStepExecutionState(globalState)];
        this._Modifications = new TracorValidatorPathModifications(this);
    }

    public bool EnableFinished { get; set; } = true;

    public IDisposable AddFinishCallback(
        Action<ITracorValidatorPath, OnTraceStepExecutionState> callback
        ) {
        var result = new CallbackDisposable(callback);
        this._ListFinishCallback = this._ListFinishCallback.Add(result);
        return result;
    }

    class CallbackDisposable : IDisposable {
        public bool IsDisposed;
        private Action<ITracorValidatorPath, OnTraceStepExecutionState>? _Callback;

        public CallbackDisposable(
            Action<ITracorValidatorPath, OnTraceStepExecutionState> callback
            ) {
            this._Callback = callback;
        }

        public void Execute(ITracorValidatorPath tracorValidatorPath, OnTraceStepExecutionState runningContextState) {
            if (this.IsDisposed) { return; }
            if (this._Callback is not { } callback) { return; }

            this._Callback(tracorValidatorPath, runningContextState);
        }

        public void Dispose() {
            this.IsDisposed = true;
        }
    }
    public IValidatorExpression Step => this._Step;

    private ValidatorStepIdentifier? _RootIdentifier;

    public void OnTrace(ITracorData tracorData) {
        var rootIdentifier = this._RootIdentifier ??= (new ValidatorStepIdentifier(0, this._Step.GetInstanceIndex()));
        var listRunningExecutionState = this._ListRunningExecutionState;
        foreach (var runningContextState in listRunningExecutionState) {
            var currentContext = new OnTraceStepCurrentContext(rootIdentifier, runningContextState, this._Modifications, this._LoggerUtility);
            var childResult = this._Step.OnTrace(tracorData, currentContext);
            if (TracorValidatorOnTraceResult.Successful == childResult) {
                this.HandleFinish(runningContextState);
            }
        }
    }

    private void HandleFinish(OnTraceStepExecutionState runningContextState) {
        if (this._ListRunningExecutionState.Contains(runningContextState)) {
            var listFinishCallback = this._ListFinishCallback;
            if (!this.EnableFinished && 0 < listFinishCallback.Length) {
                using (this._Lock.EnterScope()) {
                    this._ListRunningExecutionState = this._ListRunningExecutionState.Remove(runningContextState);
                }

                if (0 < listFinishCallback.Length) {
                    foreach (var item in listFinishCallback) {
                        item.Execute(this, runningContextState);
                    }
                }
            } else {
                using (this._Lock.EnterScope()) {
                    this._ListRunningExecutionState = this._ListRunningExecutionState.Remove(runningContextState);
                    this._ListFinishedExecutionState.Add(runningContextState);
                }

                if (EnableFinished) {
                    var oldTcs = this._TcsFinishedExecutionState;
                    this._TcsFinishedExecutionState = new TaskCompletionSource<TracorValidatorPath>();
                    oldTcs.TrySetResult(this);
                }

                if (0 < listFinishCallback.Length) {
                    foreach (var item in listFinishCallback) {
                        item.Execute(this, runningContextState);
                    }
                }
            }
        }
    }

    internal void AddFork(OnTraceStepExecutionState before, OnTraceStepExecutionState value) {
        var index = this._ListRunningExecutionState.IndexOf(before);
        if (0 <= index) {
            this._ListRunningExecutionState = this._ListRunningExecutionState.Insert(index, value);
        } else {
            this._ListRunningExecutionState = this._ListRunningExecutionState.Add(value);
        }
    }

    internal OnTraceStepExecutionState? TryGetFork(TracorForkState forkState) {
        foreach (var runningState in this._ListRunningExecutionState) {
            if (forkState.IsPartialEqual(runningState.ForkState)) {
                return runningState;
            }
        }
        return null;
    }

    internal OnTraceStepExecutionState? TryGetFork(string propertyName, TracorDataProperty tdpCurrent, Func<TracorDataProperty, TracorDataProperty, bool> fnCompare) {
        foreach (var runningState in this._ListRunningExecutionState) {
            if (runningState.ForkState.TryGetValue(propertyName, out var value)) {
                if (fnCompare(tdpCurrent, value)) { 
                    return runningState;
                }
            }
        }
        return null;
    }

    public TracorGlobalState? GetRunning(string searchSuccessState) {
        foreach (var state in this._ListRunningExecutionState) {
            if (state.GlobalState is { } globalState) {
                //if (state.DictStateByStep.ContainsKey(new ValidatorStepIdentifier( searchSuccessState))) {
                //    return globalState;
                //}
            }
        }
        return null;
    }

    public async Task<TracorGlobalState?> GetRunningAsync(string searchSuccessState, TimeSpan timeout = default) {
        {
            // quick
            var result = this.GetRunning(searchSuccessState);
            if (result is not null) {
                return result;
            }
        }
        {
            if (default == timeout) {
                if (Debugger.IsAttached) {
                    timeout = TimeSpan.FromMinutes(3);
                } else {
                    timeout = TimeSpan.FromSeconds(10);
                }
            }
        }
        if (EnableFinished) {
            using (var cts = new CancellationTokenSource()) {
                var taskExecution = this._TcsFinishedExecutionState.Task;
                // wait
                var limit = DateTime.UtcNow + timeout;
                while (DateTime.UtcNow < limit) {
                    var result = this.GetRunning(searchSuccessState);
                    if (result is not null) {
                        return result;
                    }
                    var taskDelay = Task.Delay(100, cts.Token);
                    var taskDone = await Task.WhenAny(taskDelay, taskExecution).ConfigureAwait(false);
                    if (ReferenceEquals(taskDone, taskExecution)) {
                        cts.Cancel();
                    }
                }
            }
        }
        return null;
    }


    public TracorGlobalState? GetFinished(Predicate<TracorGlobalState>? predicate = default) {
        for (var idx = 0; idx < this._ListFinishedExecutionState.Count; idx++) {
            var result = this._ListFinishedExecutionState[idx];
            if (result.GlobalState is { } globalState
               && (predicate is null || predicate(globalState))) {
                return globalState;
            }
        }
        return default;
    }

    public async Task<TracorGlobalState?> GetFinishedAsync(Predicate<TracorGlobalState>? predicate, TimeSpan timeout = default) {
        {
            // quick
            var result = this.GetFinished(predicate);
            if (result is not null) {
                return result;
            }
        }
        {
            if (default == timeout) {
                if (Debugger.IsAttached) {
                    timeout = TimeSpan.FromMinutes(3);
                } else {
                    timeout = TimeSpan.FromSeconds(10);
                }
            }
        }
        if (EnableFinished) {
            using (var cts = new CancellationTokenSource()) {
                var taskExecution = this._TcsFinishedExecutionState.Task;
                // wait
                var limit = DateTime.UtcNow + timeout;
                while (DateTime.UtcNow < limit) {
                    var result = this.GetFinished(predicate);
                    if (result is not null) {
                        return result;
                    }
                    var taskDelay = Task.Delay(100, cts.Token);
                    var taskDone = await Task.WhenAny(taskDelay, taskExecution).ConfigureAwait(false);
                    if (ReferenceEquals(taskDone, taskExecution)) {
                        cts.Cancel();
                    }
                }
            }
        }
        return null;
    }

    public List<TracorGlobalState> GetListRunning() {
        List<TracorGlobalState> result = new();
        foreach (var state in this._ListRunningExecutionState) {
            result.Add(state.GlobalState);
        }
        return result;
    }

    public List<TracorGlobalState> GetListFinished() {
        List<TracorGlobalState> result = new();
        using (this._Lock.EnterScope()) {
            foreach (var state in this._ListFinishedExecutionState) {
                result.Add(state.GlobalState);
            }
        }
        return result;
    }


    public void Dispose() {
        this._OnDispose.Dispose();
    }
}
