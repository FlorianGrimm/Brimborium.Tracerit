namespace Brimborium.Tracerit.TracorActivityListener;

internal sealed class TesttimeTracorActivityListener
    : BaseTracorActivityListener
    , ITracorActivityListener
    , IDisposable {
    private ActivityListener? _Listener;
    private readonly ITracor _Tracor;

    public TesttimeTracorActivityListener(
        IServiceProvider serviceProvider,
        ITracor tracor,
        IOptionsMonitor<TracorActivityListenerOptions> options,
        ILogger<TesttimeTracorActivityListener> logger) : base(
            serviceProvider,
            options,
            logger) {
        this._Tracor = tracor;
    }

    protected override void OnChangeOptions(TracorActivityListenerOptions options, string? name) {
        if (this._Listener is null) {
            this._LastOptions = options;
        } else {
            base.OnChangeOptions(options, name);
        }
    }

    /// <summary>
    /// add the listener.
    /// </summary>
    public void Start() {
        this.ThrowIfDisposed();

        using (this._Lock.EnterScope()) {
            if (this._Listener is { }) { return; }

            var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
            this.SetOptionState(nextOptionState);
            this.Restart();
        }
    }

    private void Restart() {
        using (var oldListener = this._Listener) {
            this._Listener = null;
        }

        ActivityListener listener = new ActivityListener() {
            ShouldListenTo = this.OnShouldListenTo,
            ActivityStarted = this.OnActivityStarted,
            ActivityStopped = this.OnActivityStopped,
            ExceptionRecorder = this.OnExceptionRecorder,
            Sample = this.OnSample,
            SampleUsingParentId = this.OnSampleUsingParentId
        };
        ActivitySource.AddActivityListener(listener);
        this._Listener = listener;
    }

    /// <summary>
    /// remove the listener
    /// </summary>
    public void Stop() {
        using (this._Lock.EnterScope()) {
            if (this._Listener is { } listener) {
                listener.Dispose();
                this._Listener = null;
            }
        }
    }

    private bool OnShouldListenTo(ActivitySource activitySource) {
        var activitySourceIdentifier = ActivitySourceIdentifier.Create(activitySource.Name, activitySource.Version);
        bool result;
        if (this._OptionState.AllowAllActivitySource) {
            result = true;
        } else {
            result = this.OnShouldListenTo(activitySourceIdentifier);
        }
        this._Logger.OnShouldListenToReturns(activitySourceIdentifier, result);
        return result;
    }

    private bool OnShouldListenTo(ActivitySourceIdentifier activitySourceIdentifier) {
        var currentOptionState = this._OptionState;
        if (currentOptionState.HashSetActivitySourceName.Contains(activitySourceIdentifier.Name)) {
            return true;
        }
        if (activitySourceIdentifier.Version is { Length: > 0 }
            && currentOptionState.HashSetActivitySourceIdenifier.Contains(activitySourceIdentifier)) {
            return true;
        }
        return false;
    }
    private void OnActivityStarted(Activity activity) {
#if false
        if (this._Listener is null || this.IsDisposed) { return; }

        if (!this._Tracor.IsGeneralEnabled()) { return; }
        if (!this._Tracor.IsCurrentlyEnabled()) { return; }

        // no locking needed since this._OptionState does not mutate
        var currentOptionState = this._OptionState;

        var activitySource = activity.Source;
        var activitySourceIdentifier = ActivitySourceIdentifier.Create(activitySource.Name, activitySource.Version);
        if (currentOptionState.AllowAllActivitySource) {
            // no check needed
        } else if (!this.OnShouldListenTo(activitySourceIdentifier)) {
            return;
        }

        if (!this._DictTracorIdentitfierCacheByActivitySource.TryGetValue(activitySourceIdentifier, out var tracorIdentitfierCache)) {
            tracorIdentitfierCache = new(new("Activity", activitySourceIdentifier.Name));
            this._DictTracorIdentitfierCacheByActivitySource = this._DictTracorIdentitfierCacheByActivitySource.Add(activitySourceIdentifier, tracorIdentitfierCache);
        }
        var tracorIdentitfier = tracorIdentitfierCache.Child("Start");
        this._Tracor.Trace(tracorIdentitfier, new ActivityTracorData(activity));
#endif
    }

    private void OnActivityStopped(Activity activity) {
        if (this._Listener is null || this.IsDisposed) { return; }

        if (!this._Tracor.IsGeneralEnabled()) { return; }
        if (!this._Tracor.IsCurrentlyEnabled()) { return; }

        // no locking needed since this._OptionState does not mutate
        var currentOptionState = this._OptionState;

        var activitySource = activity.Source;
        var activitySourceIdentifier = ActivitySourceIdentifier.Create(activitySource.Name, activitySource.Version);
        if (currentOptionState.AllowAllActivitySource) {
            // no check needed
        } else if (!this.OnShouldListenTo(activitySourceIdentifier)) {
            return;
        }

        if (!this._DictTracorIdentitfierCacheByActivitySource.TryGetValue(activitySourceIdentifier, out var tracorIdentitfierCache)) {
            tracorIdentitfierCache = new(new("Activity", activitySourceIdentifier.Name));
            this._DictTracorIdentitfierCacheByActivitySource = this._DictTracorIdentitfierCacheByActivitySource.Add(activitySourceIdentifier, tracorIdentitfierCache);
        }
        var tracorIdentitfier = tracorIdentitfierCache.Child("Stop");
        this._Tracor.Trace(tracorIdentitfier, new ActivityTracorData(activity));
    }

    private void OnExceptionRecorder(Activity activity, Exception exception, ref TagList tags) { }

    private ActivitySamplingResult OnSample(ref ActivityCreationOptions<ActivityContext> options) {
        return ActivitySamplingResult.PropagationData;
    }

    private ActivitySamplingResult OnSampleUsingParentId(ref ActivityCreationOptions<string> options) {
        return ActivitySamplingResult.PropagationData;
    }

    protected override void Dispose(bool disposing) {
        using (var optionsDispose = this._OptionsDispose) {
            if (disposing) {
                this._OptionsDispose = null;
            }
            foreach (var activitySourceBase in this._DictActivitySourceByType.Values) {
                activitySourceBase.Dispose();
            }
            this._DictActivitySourceByType.Clear();
        }
    }

    ~TesttimeTracorActivityListener() {
        this.PrepareDispose(disposing: false);
    }

    // for testing only
    public List<ActivitySourceBase> GetActivitySourceBase() {
        return this._DictActivitySourceByType.Values.ToList();
    }

    // ITracorActivityListener

    public void AddActivitySourceName(string name) {
        using (this._Lock.EnterScope()) {
            this._DirectModifications.ListActivitySourceName.Add(name);
            if (this._Listener is null) {
                //
            } else {
                var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                this.SetOptionState(nextOptionState);
                this.Restart();
            }
        }
    }

    public void RemoveActivitySourceName(string name) {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceName.Remove(name)) {
                if (this._Listener is null) {
                    //
                } else {
                    var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                    this.SetOptionState(nextOptionState);
                    this.Restart();
                }
            }
        }
    }

    public void AddActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier) {
        using (this._Lock.EnterScope()) {
            this._DirectModifications.ListActivitySourceIdenifier.Add(activitySourceIdentifier);
            if (this._Listener is null) {
                //
            } else {
                var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                this.SetOptionState(nextOptionState);
                this.Restart();
            }
        }
    }

    public void RemoveActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier) {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceIdenifier.Remove(activitySourceIdentifier)) {
                if (this._Listener is null) {
                    //
                } else {
                    var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                    this.SetOptionState(nextOptionState);
                    this.Restart();
                }
            }
        }
    }
}
