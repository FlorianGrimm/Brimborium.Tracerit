using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace Brimborium.Tracerit.TracorActivityListener;

internal sealed class TesttimeTracorActivityListener : ITracorActivityListener, IDisposable {
    private readonly Lock _Lock = new();
    private readonly ITracor _Tracor;
    private readonly IOptionsMonitor<TracorActivityListenerOptions> _Options;
    private readonly ILogger<TesttimeTracorActivityListener> _Logger;
    private ImmutableDictionary<ActivitySourceIdenifier, TracorIdentitfierCache> _DictTracorIdentitfierCacheByActivitySource = ImmutableDictionary<ActivitySourceIdenifier, TracorIdentitfierCache>.Empty;
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
        public readonly HashSet<ActivitySourceIdenifier> HashSetActivitySourceIdenifier;

        public OptionState() {
            this.HashSetActivitySourceName = new HashSet<string>(StringComparer.Ordinal);
            this.HashSetActivitySourceIdenifier = new HashSet<ActivitySourceIdenifier>();
        }
        public static OptionState Create(TracorActivityListenerOptions options1, TracorActivityListenerOptions options2) {
            var result = new OptionState() {
                AllowAllActivitySource = options1.AllowAllActivitySource || options2.AllowAllActivitySource,
            };

            addListActivitySourceName(options1, result);
            addListActivitySourceName(options2, result);
            addListActivitySourceIdenifier(options1, result);
            addListActivitySourceIdenifier(options2, result);

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
        }
    }

    public TesttimeTracorActivityListener(
        ITracor tracor,
        IOptionsMonitor<TracorActivityListenerOptions> options,
        ILogger<TesttimeTracorActivityListener> logger) {
        this._Tracor = tracor;
        this._Options = options;
        this._Logger = logger;
        this._OptionsDispose = this._Options.OnChange(this._OnChangeOptions);
    }

    private void _OnChangeOptions(TracorActivityListenerOptions options, string? name) {
        using (this._Lock.EnterScope()) {
            var nextOptionState = OptionState.Create(options, this._DirectModifications);
            this._LastOptions = options;
            this.SetOptionState(nextOptionState);
        }
    }

    private void SetOptionState(OptionState value) {
        this._OptionState = value;
    }

    public void Start() {
        if (this._IsDisposed is { }) { throw new ObjectDisposedException(nameof(TesttimeTracorActivityListener), this._IsDisposed); }

        using (this._Lock.EnterScope()) {
            if (this._Listener is { }) { return; }

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

    public void Stop() {
        using (this._Lock.EnterScope()) {
            if (this._Listener is { } listener) {
                listener.Dispose();
                this._Listener = null;
            }
        }
    }

    private bool OnShouldListenTo(ActivitySource activitySource) {
        var activitySourceIdenifier = ActivitySourceIdenifier.Create(activitySource.Name, activitySource.Version);
        return this.OnShouldListenTo(activitySourceIdenifier);
    }

    private bool OnShouldListenTo(ActivitySourceIdenifier activitySourceIdenifier) {
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
        var activitySourceIdenifier = ActivitySourceIdenifier.Create(activitySource.Name, activitySource.Version);
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
        var activitySourceIdenifier = ActivitySourceIdenifier.Create(activitySource.Name, activitySource.Version);
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
            var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
            this.SetOptionState(nextOptionState);
        }
    }

    public void RemoveActivitySourceName(string name) {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceName.Remove(name)) {
                var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                this.SetOptionState(nextOptionState);
            }
        }
    }

    public void AddListInstrumentation(ActivitySourceIdenifier instrumentationRegistration) {
        using (this._Lock.EnterScope()) {
            this._DirectModifications.ListActivitySourceIdenifier.Add(instrumentationRegistration);
            var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
            this.SetOptionState(nextOptionState);
        }
    }

    public void RemoveListInstrumentation(ActivitySourceIdenifier instrumentationRegistration) {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceIdenifier.Remove(instrumentationRegistration)) {
                var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                this.SetOptionState(nextOptionState);
            }
        }
    }
}
