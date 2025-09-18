namespace Brimborium.Tracerit.TracorActivityListener;

internal sealed class TesttimeTracorActivityListener : ITracorActivityListener, IDisposable {
    private readonly Lock _Lock = new();
    private readonly IServiceProvider _ServiceProvider;
    private readonly ITracor _Tracor;
    private readonly Dictionary<Type, ActivitySourceBase> _DictActivitySourceByType = new();
    private readonly IOptionsMonitor<TracorActivityListenerOptions> _Options;
    private readonly ILogger<TesttimeTracorActivityListener> _Logger;
    private ImmutableDictionary<ActivitySourceIdentifier, TracorIdentitfierCache> _DictTracorIdentitfierCacheByActivitySource = ImmutableDictionary<ActivitySourceIdentifier, TracorIdentitfierCache>.Empty;
    private IDisposable? _OptionsDispose;
    private ActivityListener? _Listener;
    private string? _IsDisposed;
    private TracorActivityListenerOptions _LastOptions = new();
    private TracorActivityListenerOptions _DirectModifications = new();
    private OptionState _OptionState = new();

    private class OptionState {
        // do not mutate
        public bool AllowAllActivitySource { get; init; }
        public readonly HashSet<string> HashSetActivitySourceName;
        public readonly HashSet<ActivitySourceIdentifier> HashSetActivitySourceIdenifier;
        public readonly HashSet<Type> HashSetActivitySourceByType = new();

        public OptionState() {
            this.HashSetActivitySourceName = new HashSet<string>(StringComparer.Ordinal);
            this.HashSetActivitySourceIdenifier = new HashSet<ActivitySourceIdentifier>();
        }
        public static OptionState Create(TracorActivityListenerOptions options1, TracorActivityListenerOptions options2) {
            var result = new OptionState() {
                AllowAllActivitySource = options1.AllowAllActivitySource || options2.AllowAllActivitySource,
            };

            addListActivitySourceName(options1, result);
            addListActivitySourceName(options2, result);
            addListActivitySourceIdenifier(options1, result);
            addListActivitySourceIdenifier(options2, result);
            addListActivitySourceByType(options1, result);
            addListActivitySourceByType(options2, result);

            return result;

            static void addListActivitySourceName(TracorActivityListenerOptions options, OptionState result) {
                foreach (var activitySourceName in options.ListActivitySourceName) {
                    result.HashSetActivitySourceName.Add(activitySourceName);
                }
            }
            static void addListActivitySourceIdenifier(TracorActivityListenerOptions options, OptionState result) {
                foreach (var instrumentation in options.ListActivitySourceIdenifier) {
                    if (instrumentation.Version is { Length: 0 }) {
                        result.HashSetActivitySourceName.Add(instrumentation.Name);
                    } else {
                        result.HashSetActivitySourceIdenifier.Add(instrumentation);
                    }
                }
            }
            static void addListActivitySourceByType(TracorActivityListenerOptions options, OptionState result) {
                foreach (var type in options.ListActivitySourceByType) {
                    result.HashSetActivitySourceByType.Add(type);
                }
            }
        }
    }

    public TesttimeTracorActivityListener(
        IServiceProvider serviceProvider,
        ITracor tracor,
        IOptionsMonitor<TracorActivityListenerOptions> options,
        ILogger<TesttimeTracorActivityListener> logger) {
        this._ServiceProvider = serviceProvider;
        this._Tracor = tracor;
        this._Options = options;
        this._Logger = logger;
        this._LastOptions = this._Options.CurrentValue;
        this._OptionsDispose = this._Options.OnChange(this.OnChangeOptions);
    }

    private void OnChangeOptions(TracorActivityListenerOptions options, string? name) {
        if (this._Listener is null) {
            this._LastOptions = options;
        } else { 
            using (this._Lock.EnterScope()) {
                var nextOptionState = OptionState.Create(options, this._DirectModifications);
                this._LastOptions = options;
                this.SetOptionState(nextOptionState);
            }
        }
    }

    private void SetOptionState(OptionState value) {
        this._OptionState = value;
        foreach (var type in value.HashSetActivitySourceByType) {
            if (this._DictActivitySourceByType.ContainsKey(type)) {
                continue;
            }
            try {
                var service = this._ServiceProvider.GetService(type);
                if (service is null) {
                    var configuration = this._ServiceProvider.GetRequiredService<IConfiguration>();
                    service = ActivatorUtilities.CreateInstance(this._ServiceProvider, type, configuration);
                }
                if (service is ActivitySourceBase activitySource) {
                    this._DictActivitySourceByType[type] = activitySource;
                    continue;
                }
                {
                    this._Logger.LogError(
                        message: "Failed to create ActivitySourceBase. Service is not of type ActivitySourceBase. {ServiceType}",
                        service.GetType().FullName);
                }
            } catch (Exception ex) {
                this._Logger.LogError(exception: ex, message: "Failed to create ActivitySourceBase");
            }
        }
    }

    /// <summary>
    /// add the listener.
    /// </summary>
    public void Start() {
        if (this._IsDisposed is { }) { throw new ObjectDisposedException(nameof(TesttimeTracorActivityListener), this._IsDisposed); }

        using (this._Lock.EnterScope()) {
            if (this._Listener is { }) { return; }

            var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
            this.SetOptionState(nextOptionState);

            ActivityListener listener = new ActivityListener() {
                ShouldListenTo = this.OnShouldListenTo,
                ActivityStarted = this.OnActivityStarted,
                ActivityStopped = this.OnActivityStopped,
                ExceptionRecorder = this.OnExceptionRecorder,
            };
            ActivitySource.AddActivityListener(listener);
            this._Listener = listener;
        }
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
        var activitySourceIdenifier = ActivitySourceIdentifier.Create(activitySource.Name, activitySource.Version);
        return this.OnShouldListenTo(activitySourceIdenifier);
    }

    private bool OnShouldListenTo(ActivitySourceIdentifier activitySourceIdenifier) {
        var currentOptionState = this._OptionState;
        if (currentOptionState.HashSetActivitySourceName.Contains(activitySourceIdenifier.Name)) {
            return true;
        }
        if (activitySourceIdenifier.Version is { Length: > 0 }
            && currentOptionState.HashSetActivitySourceIdenifier.Contains(activitySourceIdenifier)) {
        }
        return false;
    }
    private void OnActivityStarted(Activity activity) {
        if (this._IsDisposed is { }) { return; }
        if (!this._Tracor.IsGeneralEnabled()) { return; }
        if (!this._Tracor.IsCurrentlyEnabled()) { return; }

        // no locking needed since this._OptionState does not mutate
        var currentOptionState = this._OptionState;

        var activitySource = activity.Source;
        var activitySourceIdenifier = ActivitySourceIdentifier.Create(activitySource.Name, activitySource.Version);
        if (currentOptionState.AllowAllActivitySource) {
            // no check needed
        } else if (!this.OnShouldListenTo(activitySourceIdenifier)) {
            return;
        }

        if (!this._DictTracorIdentitfierCacheByActivitySource.TryGetValue(activitySourceIdenifier, out var tracorIdentitfierCache)) {
            tracorIdentitfierCache = new(new("Activity", activitySourceIdenifier.Name));
            this._DictTracorIdentitfierCacheByActivitySource = this._DictTracorIdentitfierCacheByActivitySource.Add(activitySourceIdenifier, tracorIdentitfierCache);
        }
        var tracorIdentitfier = tracorIdentitfierCache.Child("Start");
        this._Tracor.Trace(tracorIdentitfier, new ActivityTracorData(activity));
    }

    private void OnActivityStopped(Activity activity) {
        if (this._IsDisposed is { }) { return; }
        if (!this._Tracor.IsGeneralEnabled()) { return; }
        if (!this._Tracor.IsCurrentlyEnabled()) { return; }

        // no locking needed since this._OptionState does not mutate
        var currentOptionState = this._OptionState;

        var activitySource = activity.Source;
        var activitySourceIdenifier = ActivitySourceIdentifier.Create(activitySource.Name, activitySource.Version);
        if (currentOptionState.AllowAllActivitySource) {
            // no check needed
        } else if (!this.OnShouldListenTo(activitySourceIdenifier)) {
            return;
        }

        if (!this._DictTracorIdentitfierCacheByActivitySource.TryGetValue(activitySourceIdenifier, out var tracorIdentitfierCache)) {
            tracorIdentitfierCache = new(new("Activity", activitySourceIdenifier.Name));
            this._DictTracorIdentitfierCacheByActivitySource = this._DictTracorIdentitfierCacheByActivitySource.Add(activitySourceIdenifier, tracorIdentitfierCache);
        }
        var tracorIdentitfier = tracorIdentitfierCache.Child("Stop");
        this._Tracor.Trace(tracorIdentitfier, new ActivityTracorData(activity));
    }

    private void OnExceptionRecorder(Activity activity, Exception exception, ref TagList tags) { }

    private void Dispose(bool disposing) {
        this._IsDisposed = Environment.StackTrace;
        using (var optionsDispose = this._OptionsDispose) {
            if (disposing) {
                this._OptionsDispose = null;
            }
        }
    }

    ~TesttimeTracorActivityListener() {
        this.Dispose(disposing: false);
    }

    public void Dispose() {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void AddActivitySourceName(string name) {
        using (this._Lock.EnterScope()) {
            this._DirectModifications.ListActivitySourceName.Add(name);
            if (this._Listener is null) {
                //
            } else {
                var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                this.SetOptionState(nextOptionState);
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
                }
            }
        }
    }

    public void AddActivitySourceByType<T>() where T : ActivitySourceBase {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceByType.Contains(typeof(T))) {
                // already added
            } else {
                this._DirectModifications.ListActivitySourceByType.Add(typeof(T));
                if (this._Listener is null) {
                    //
                } else {
                    var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                    this.SetOptionState(nextOptionState);
                }
            }
        }
    }

    public void RemoveActivitySourceByType<T>() where T : ActivitySourceBase {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceByType.Remove(typeof(T))) {
                if (this._Listener is null) {
                    //
                } else {
                    var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                    this.SetOptionState(nextOptionState);
                }
            }
        }
    }
    

    // for testing only
    public List<ActivitySourceBase> GetActivitySourceBase() {
        return this._DictActivitySourceByType.Values.ToList();
    }
}
