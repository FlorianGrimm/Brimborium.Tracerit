using Brimborium.Tracerit.Diagnostics;

namespace Brimborium.Tracerit.TracorActivityListener;

internal abstract class BaseTracorActivityListener
    : IDisposable {
    protected class OptionState {
        // do not mutate
        public required bool ActivitySourceStartEventEnabled;
        public required bool ActivitySourceStopEventEnabled;
        public required bool AllowAllActivitySource;
        public readonly HashSet<string> HashSetActivitySourceName;
        public readonly HashSet<ActivitySourceIdentifier> HashSetActivitySourceIdenifier;
        public readonly HashSet<IActivitySourceResolver> HashSetActivitySourceResolver = new();

        public OptionState() {
            this.HashSetActivitySourceName = new HashSet<string>(StringComparer.Ordinal);
            this.HashSetActivitySourceIdenifier = new HashSet<ActivitySourceIdentifier>();
        }
        public static OptionState Create(TracorActivityListenerOptions options1, TracorActivityListenerOptions options2) {
            var result = new OptionState() {
                ActivitySourceStartEventEnabled = options1.ActivitySourceStartEventEnabled || options2.ActivitySourceStartEventEnabled,
                ActivitySourceStopEventEnabled = options1.ActivitySourceStopEventEnabled || options2.ActivitySourceStopEventEnabled,
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
                foreach (var resolver in options.ListActivitySourceResolver) {
                    result.HashSetActivitySourceResolver.Add(resolver);
                }
            }
        }
    }
    private string? _IsDisposed;
    protected readonly Lock _Lock = new();
    protected readonly IServiceProvider _ServiceProvider;
    protected readonly IOptionsMonitor<TracorActivityListenerOptions> _Options;
    protected readonly ILogger _Logger;
    protected ImmutableDictionary<ActivitySourceIdentifier, TracorIdentitfierCache> _DictTracorIdentitfierCacheByActivitySource = ImmutableDictionary<ActivitySourceIdentifier, TracorIdentitfierCache>.Empty;
    protected IDisposable? _OptionsDispose;

    protected TracorActivityListenerOptions _LastOptions = new();
    protected TracorActivityListenerOptions _DirectModifications = new();
    protected OptionState _OptionState = new() {
        ActivitySourceStartEventEnabled = false,
        ActivitySourceStopEventEnabled = true,
        AllowAllActivitySource = false,
    };

    public BaseTracorActivityListener(
        IServiceProvider serviceProvider,
        IOptionsMonitor<TracorActivityListenerOptions> options,
        ILogger logger) {
        this._ServiceProvider = serviceProvider;
        this._Options = options;
        this._Logger = logger;
        this._LastOptions = this._Options.CurrentValue;
        this._OptionsDispose = this._Options.OnChange(this.OnChangeOptions);
    }

    protected virtual void OnChangeOptions(TracorActivityListenerOptions options, string? name) {
        using (this._Lock.EnterScope()) {
            var nextOptionState = OptionState.Create(options, this._DirectModifications);
            this._LastOptions = options;
            this.SetOptionState(nextOptionState);
        }
    }

    protected void SetOptionState(OptionState value) {
        this._OptionState = value;
        foreach (var activitySourceResolver in value.HashSetActivitySourceResolver) {
            if (activitySourceResolver.Resolve(this._ServiceProvider) is ActivitySource activitySource) {
                value.HashSetActivitySourceName.Add(activitySource.Name);
                value.HashSetActivitySourceIdenifier.Add(new ActivitySourceIdentifier(activitySource.Name, activitySource.Version));
                continue;
            }
        }
    }

    protected virtual void Dispose(bool disposing) {
    }

    protected bool IsDisposed => this._IsDisposed is not null;

    protected void ThrowIfDisposed() {
        if (this._IsDisposed is { } stacktrace) {
            throw new ObjectDisposedException(this.GetType().Name, stacktrace);
        }
    }

    protected void PrepareDispose(bool disposing) {
        if (this._IsDisposed is null) {
            if (disposing) {
                this._IsDisposed = Environment.StackTrace;
            } else {
                this._IsDisposed = "finallizer";
            }
            this.Dispose(disposing);
        }
    }

    ~BaseTracorActivityListener() {
        this.PrepareDispose(disposing: false);
    }

    public void Dispose() {
        this.PrepareDispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
}
