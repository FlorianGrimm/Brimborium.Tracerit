namespace Brimborium.Tracerit.Service;

internal sealed class TracorValidatorPath : ITracorValidatorPath {
    private readonly Lock _Lock = new();
    private readonly IValidatorExpression _Step;
    private readonly IDisposable _OnDispose;
    private readonly LoggerUtility _LoggerUtility;
    private readonly TracorValidatorPathModifications _Modifications;
    private ImmutableArray<OnTraceStepExecutionState> _ListRunningExecutionState;
    private readonly List<TracorFinishState> _ListFinishState = new();
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
        Action<ITracorValidatorPath, TracorFinishState> callback
        ) {
        var result = new CallbackDisposable(callback);
        this._ListFinishCallback = this._ListFinishCallback.Add(result);
        return result;
    }

    private class CallbackDisposable : IDisposable {
        public bool IsDisposed;
        private Action<ITracorValidatorPath, TracorFinishState>? _Callback;

        public CallbackDisposable(
            Action<ITracorValidatorPath, TracorFinishState> callback
            ) {
            this._Callback = callback;
        }

        public void Execute(ITracorValidatorPath tracorValidatorPath, TracorFinishState finishState) {
            if (this.IsDisposed) { return; }
            
            if (this._Callback is not { } callback) { return; }
            callback(tracorValidatorPath, finishState);
        }

        public void Dispose() {
            this.IsDisposed = true;
            this._Callback = null;
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
            if (childResult.IsComplete()) {
                this.HandleFinish(runningContextState, childResult);
            }
        }
    }

    private void HandleFinish(OnTraceStepExecutionState runningContextState, TracorValidatorOnTraceResult finalResult) {
        if (this._ListRunningExecutionState.Contains(runningContextState)) {
            var finishState = runningContextState.GetFinishState(finalResult);
            var listFinishCallback = this._ListFinishCallback;
            if (!this.EnableFinished && 0 < listFinishCallback.Length) {
                using (this._Lock.EnterScope()) {
                    this._ListRunningExecutionState = this._ListRunningExecutionState.Remove(runningContextState);
                }
                if (0 < listFinishCallback.Length) {
                    foreach (var item in listFinishCallback) {
                        item.Execute(this, finishState);
                    }
                }
            } else {
                using (this._Lock.EnterScope()) {
                    this._ListRunningExecutionState = this._ListRunningExecutionState.Remove(runningContextState);
                    this._ListFinishState.Add(finishState);
                }

                if (this.EnableFinished) {
                    var oldTcs = this._TcsFinishedExecutionState;
                    this._TcsFinishedExecutionState = new TaskCompletionSource<TracorValidatorPath>();
                    oldTcs.TrySetResult(this);
                }

                if (0 < listFinishCallback.Length) {
                    foreach (var item in listFinishCallback) {
                        item.Execute(this, finishState);
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

    internal OnTraceStepExecutionState? TryGetFork(in TracorForkState forkState) {
        foreach (var runningState in this._ListRunningExecutionState) {
            if (forkState.IsPartialEqual(runningState.ForkState)) {
                return runningState;
            }
        }
        return null;
    }

    internal OnTraceStepExecutionState? TryGetFork(in TracorDataProperty tdpCurrent, Func<TracorDataProperty, TracorDataProperty, bool> fnCompare) {
        foreach (var runningState in this._ListRunningExecutionState) {
            if (runningState.ForkState.TryGetValue(tdpCurrent.Name, out var value)) {
                if (fnCompare(tdpCurrent, value)) { 
                    return runningState;
                }
            }
        }
        return null;
    }

    public TracorRunningState? GetRunning(string searchSuccessState) {
        using (this._Lock.EnterScope()) {
            foreach (var state in this._ListRunningExecutionState) {
#warning GetRunning
                //if (state.GlobalState is { } globalState) {
                return state.GetTracorRunningState();

                //if (state.DictStateByStep.ContainsKey(new ValidatorStepIdentifier( searchSuccessState))) {
                //    return globalState;
                //}
                //}
            } 
        }
        return null;
    }

    public async Task<TracorRunningState?> GetRunningAsync(string searchSuccessState, TimeSpan timeout = default) {
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
        if (this.EnableFinished) {
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


    public TracorFinishState? GetFinished(Predicate<TracorFinishState>? predicate = default) {
        for (var index = 0; index < this._ListFinishState.Count; index++) {
            var finishState = this._ListFinishState[index];
            if (predicate is null || predicate(finishState)) {
                return finishState;
            }
        }
        return default;
    }

    public async Task<TracorFinishState?> GetFinishedAsync(Predicate<TracorFinishState>? predicate, TimeSpan timeout = default) {
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
        if (this.EnableFinished) {
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

    public List<TracorRunningState> GetListRunning() {
        List<TracorRunningState> result = new();
        using (this._Lock.EnterScope()) {
            foreach (var state in this._ListRunningExecutionState) {
                result.Add(state.GetTracorRunningState());
            }
        }
        return result;
    }

    public List<TracorFinishState> GetListFinished() {
        List<TracorFinishState> result = new();
        using (this._Lock.EnterScope()) {
            foreach (var state in this._ListFinishState) {
                result.Add(state);
            }
        }
        return result;
    }


    public void Dispose() {
        this._OnDispose.Dispose();
    }
}
