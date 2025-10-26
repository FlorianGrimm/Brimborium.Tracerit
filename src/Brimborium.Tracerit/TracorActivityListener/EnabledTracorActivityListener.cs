namespace Brimborium.Tracerit.TracorActivityListener;

internal sealed class EnabledTracorActivityListener
    : BaseTracorActivityListener
    , ITracorActivityListener
    , IDisposable {
    internal static EnabledTracorActivityListener Create(
        IServiceProvider serviceProvider,
        IConfiguration? configuration
        ) {
        //TODO
        //if (configuration is null) {
        //    serviceProvider.GetRequiredService<IConfiguration>().GetSection("");
        //}
        var activityTracorDataPool = serviceProvider.GetRequiredService<ActivityTracorDataPool>();
        var tracorCollectiveSink = serviceProvider.GetRequiredService<ITracorCollectiveSink>();
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<TracorActivityListenerOptions>>();
        var logger = serviceProvider.GetRequiredService<ILogger<EnabledTracorActivityListener>>();
        return new EnabledTracorActivityListener(
            serviceProvider,
            activityTracorDataPool,
            tracorCollectiveSink,
            options,
            logger);
    }

    private ActivityListener? _Listener;
    private readonly ActivityTracorDataPool _ActivityTracorDataPool;
    private readonly ITracorCollectiveSink _TracorCollectiveSink;

    public EnabledTracorActivityListener(
        IServiceProvider serviceProvider,
        ActivityTracorDataPool activityTracorDataPool,
        ITracorCollectiveSink tracorCollectiveSink,
        IOptionsMonitor<TracorActivityListenerOptions> options,
        ILogger<EnabledTracorActivityListener> logger) : base(
            serviceProvider,
            options,
            logger) {
        this._ActivityTracorDataPool = activityTracorDataPool;
        this._TracorCollectiveSink = tracorCollectiveSink;
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
        var activitySourceIdentifier = ActivitySourceIdentifier.Create(activitySource);
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

        if (this._Listener is null || this.IsDisposed) { return; }

        // no locking needed since this._OptionState does not mutate
        var currentOptionState = this._OptionState;

        if (!currentOptionState.ActivitySourceStartEventEnabled) { return; }

        if (!this._TracorCollectiveSink.IsGeneralEnabled()) { return; }
        if (!this._TracorCollectiveSink.IsEnabled()) { return; }

        var activitySourceIdentifier = ActivitySourceIdentifier.Create(activity.Source);
        if (currentOptionState.AllowAllActivitySource) {
            // no check needed
        } else if (!this.OnShouldListenTo(activitySourceIdentifier)) {
            return;
        }

        if (!this._DictTracorIdentifierCacheByActivitySource.TryGetValue(activitySourceIdentifier, out var tracorIdentifierCache)) {
            tracorIdentifierCache = new(new("Activity", activitySourceIdentifier.Name));
            this._DictTracorIdentifierCacheByActivitySource = this._DictTracorIdentifierCacheByActivitySource.Add(activitySourceIdentifier, tracorIdentifierCache);
        }
        var tracorIdentifier = tracorIdentifierCache.Child("Start");

        using (var activityTracorData = this._ActivityTracorDataPool.Rent()) {
            activityTracorData.SetValue(activity);
            activityTracorData.TracorIdentifier = tracorIdentifier;
            activityTracorData.Timestamp = activity.StartTimeUtc;
            this._TracorCollectiveSink.OnTrace(true, activityTracorData);
        }
    }

    private void OnActivityStopped(Activity activity) {
        if (this._Listener is null || this.IsDisposed) { return; }

        // no locking needed since this._OptionState does not mutate
        var currentOptionState = this._OptionState;

        if (!currentOptionState.ActivitySourceStopEventEnabled) { return; }

        if (!this._TracorCollectiveSink.IsGeneralEnabled()) { return; }
        if (!this._TracorCollectiveSink.IsEnabled()) { return; }

        var activitySourceIdentifier = ActivitySourceIdentifier.Create(activity.Source);
        if (currentOptionState.AllowAllActivitySource) {
            // no check needed
        } else if (!this.OnShouldListenTo(activitySourceIdentifier)) {
            return;
        }

        if (!this._DictTracorIdentifierCacheByActivitySource.TryGetValue(activitySourceIdentifier, out var tracorIdentifierCache)) {
            tracorIdentifierCache = new(new("Activity", activitySourceIdentifier.Name));
            this._DictTracorIdentifierCacheByActivitySource = this._DictTracorIdentifierCacheByActivitySource.Add(activitySourceIdentifier, tracorIdentifierCache);
        }

        var tracorIdentifier = tracorIdentifierCache.Child("Stop");
        using (var activityTracorData = this._ActivityTracorDataPool.Rent()) {
            activityTracorData.SetValue(activity);
            activityTracorData.TracorIdentifier = tracorIdentifier;
            activityTracorData.Timestamp = activity.StartTimeUtc.Add(activity.Duration);
            this._TracorCollectiveSink.OnTrace(true, activityTracorData);
        }
    }

    private void OnExceptionRecorder(Activity activity, Exception exception, ref TagList tags) { }

    private readonly Dictionary<ActivitySourceIdentifier, ActivitySamplingResult> _OnSampleActivitySamplingResult = new();

    private ActivitySamplingResult OnSample(ref ActivityCreationOptions<ActivityContext> options) {
        var activitySourceIdentifier = ActivitySourceIdentifier.Create(options.Source);
        ActivitySamplingResult result;
        // TODO: Configure named SamplingResult
        if (this._OnSampleActivitySamplingResult.TryGetValue(activitySourceIdentifier, out result)) {
            return result;
        } else {
            // TODO: Configure default SamplingResult
            return ActivitySamplingResult.AllDataAndRecorded;
        }
    }

    private readonly Dictionary<ActivitySourceIdentifier, ActivitySamplingResult> _OnSampleUsingParentIdActivitySamplingResult = new();

    private ActivitySamplingResult OnSampleUsingParentId(ref ActivityCreationOptions<string> options) {
        var activitySourceIdentifier = ActivitySourceIdentifier.Create(options.Source);
        ActivitySamplingResult result;
        // TODO: Configure named SamplingResult
        if (this._OnSampleUsingParentIdActivitySamplingResult.TryGetValue(activitySourceIdentifier, out result)) {
            return result;
        } else {
            // TODO: Configure default SamplingResult
            return ActivitySamplingResult.AllDataAndRecorded;
        }
    }

    protected override void Dispose(bool disposing) {
        using (var listener = this._Listener) {
            using (var optionsDispose = this._OptionsDispose) {
                if (disposing) {
                    this._OptionsDispose = null;
                    this._Listener = null;
                }
            }
        }
    }

    ~EnabledTracorActivityListener() {
        this.PrepareDispose(disposing: false);
    }

    public List<ActivitySourceIdentifier> GetListActivitySourceIdentifier() {
        return this._OptionState.HashSetActivitySourceIdenifier.ToList();
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
