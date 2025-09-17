namespace Brimborium.Tracerit;

/// <summary>
/// 
/// </summary>
/// <example>
/// [DisplayName(XYZActivitySource.ActivitySourceName)]
/// public partial class XYZActivitySource : ActivitySourceEx {
///     public const string ActivitySourceName = "XYZ";
///     public XYZActivitySource(IConfiguration? configuration=default) : base(configuration, ActivitySourceName) { }
///     public XYZActivitySource() : base(null, ActivitySourceName) { }
/// }
/// </example>
public abstract class ActivitySourceBase {
    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    protected ActivitySourceBase(
        IConfiguration? configuration,
        string sourceName,
        string? sourceVersion = default,
        LogLevel? logLevel
        ) {
        if (string.IsNullOrEmpty(sourceName)) { throw new ArgumentNullException(nameof(sourceName)); }
        this.SourceName = sourceName;
        this.SourceVersion = sourceVersion;
        this._ActivitySource = new ActivitySource(this.SourceName);
        if (logLevel is { } logLevelValue) {
            this._LogLevel = logLevelValue;
        }
        if (configuration is not null) {
            ChangeToken.OnChange(
                () => configuration.GetSection("ActivitySource").GetReloadToken(),
                (sectionActivitySource) => this.SetLogLevel(sectionActivitySource),
                configuration.GetSection("ActivitySource")
                );
        }
    }

    public void SetLogLevel(IConfigurationSection sectionActivitySource) {
        var value = sectionActivitySource.GetValue<string>(this.SourceName, string.Empty);
        this.SetLogLevel(value);
    }

    protected LogLevel _LogLevel = LogLevel.Information;

    protected ActivitySource _ActivitySource;

    public ActivitySource ActivitySource => this._ActivitySource;

    public string SourceName { get; }
    public string? SourceVersion { get; }

    public void SetLogLevel(LogLevel level) { this._LogLevel = level; }

    public LogLevel LogLevel => this._LogLevel;

    public void SetIsEnabled(bool value) {
        this._LogLevel = (value) ? LogLevel.Trace : LogLevel.None;
    }

    private static Dictionary<string, LogLevel>? _LogLevelByName;
    private static Dictionary<string, LogLevel> GetLogLevelByName()
        => _LogLevelByName ??= new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase) {
            { "Trace", LogLevel.Trace },
            { "Debug", LogLevel.Debug },
            { "Information", LogLevel.Information },
            { "Warning", LogLevel.Warning },
            { "Error", LogLevel.Error },
            { "Critical", LogLevel.Critical },
            { "None", LogLevel.None },
            { "True", LogLevel.Information },
            { "False", LogLevel.None },
            { "Enable", LogLevel.Information },
            { "Disable", LogLevel.None }
        };

    public void SetLogLevel(string? value) {
        if (string.IsNullOrEmpty(value)) {
            this._LogLevel = LogLevel.Information;
            return;
        }
        var logLevelByName = GetLogLevelByName();

        if (logLevelByName.TryGetValue(value, out var result)) {
            this._LogLevel = result;
        } else {
            this._LogLevel = LogLevel.Information;
        }
    }

    public bool IsEnabled(LogLevel logLevel) {
        if (!this._ActivitySource.HasListeners()) { return false; }
        if (LogLevel.None <= logLevel) { return false; }
        if (LogLevel.None <= this._LogLevel) { return false; }

        if (logLevel >= this._LogLevel) {
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.
    /// </summary>
    /// <param name="kind">The <see cref="ActivityKind"/></param>
    /// <param name="parentContext">The parent <see cref="ActivityContext"/> object to initialize the created Activity object with.</param>
    /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
    /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
    /// <param name="startTime">The optional start timestamp to set on the created Activity object.</param>
    /// <param name="logLevel">The optional log level. Default is Information</param>
    /// <param name="name">The operation name of the Activity.</param>
    /// <returns>The created <see cref="Activity"/> object or null if there is no any listener.</returns>
    public Activity? StartActivity(ActivityKind kind = ActivityKind.Internal, ActivityContext parentContext = default, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, DateTimeOffset startTime = default, LogLevel logLevel = LogLevel.Information, [CallerMemberName] string name = "") {
        if (this.IsEnabled(logLevel)) {
            return this.ActivitySource.StartActivity(kind, parentContext, tags, links, startTime, name);
        } else {
            return default;
        }
    }

    public RestoreRootActivity? StartRootActivity(ActivityKind kind = ActivityKind.Internal, ActivityContext parentContext = default, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, DateTimeOffset startTime = default, LogLevel logLevel = LogLevel.Information, [CallerMemberName] string name = "") {
        if (this.IsEnabled(logLevel)) {
            // side effect: Activity.Current = default;
            RestoreRootActivity result = new ();

            // side effect: Activity.Current = result.RootActivity;
            result.RootActivity = this.ActivitySource.StartActivity(kind, parentContext, tags, links, startTime, name);

            return result;
        } else {
            return default;
        }
    }
}

public struct RestoreRootActivity : IDisposable {
    private readonly Activity? _Previous;
    private Activity? _RootActivity;
    
    public Activity? RootActivity {
        get => this._RootActivity;
        set {
            this._RootActivity = value;
            Activity.Current = value;
        }
    }

    public RestoreRootActivity() {
        this._Previous = Activity.Current;
        Activity.Current = default;
    }

    public void Dispose() {
        try {
            using (var rootActivity = this.RootActivity) {
                this.RootActivity = null;
                rootActivity?.Stop();
            }
            if (this._Previous is not null && !this._Previous.IsStopped) {
                Activity.Current = this._Previous;
            }
        } catch {
        }
    }
}
