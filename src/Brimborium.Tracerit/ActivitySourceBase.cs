namespace Brimborium.Tracerit;

/// <summary>
/// Provides a base class for creating activity sources with configurable logging levels and activity management.
/// This abstract class wraps the .NET <see cref="ActivitySource"/> with additional functionality for log level control,
/// configuration-based settings, and specialized activity creation methods.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ActivitySourceBase"/> class serves as a foundation for implementing custom activity sources
/// that integrate with the .NET distributed tracing system. It provides:
/// </para>
/// <list type="bullet">
/// <item><description>Configuration-based log level management with automatic reloading</description></item>
/// <item><description>Activity creation with log level filtering</description></item>
/// <item><description>Root activity management with context restoration</description></item>
/// <item><description>Integration with dependency injection containers</description></item>
/// </list>
/// <para>
/// Derived classes should implement specific activity sources for different components or services,
/// providing a consistent interface for distributed tracing across the application.
/// </para>
/// </remarks>
/// <example>
/// <para>Creating a custom activity source:</para>
/// <code>
/// [DisplayName(XYZActivitySource.ActivitySourceName)]
/// public partial class XYZActivitySource : ActivitySourceBase {
///     public const string ActivitySourceName = "XYZ";
///     public XYZActivitySource(IConfiguration? configuration = default)
///         : base(configuration, ActivitySourceName) { }
///     public XYZActivitySource() : base(null, ActivitySourceName) { }
/// }
/// </code>
/// <para>Using the activity source:</para>
/// <code>
/// using var activity = activitySource.StartActivity();
/// activity?.SetTag("operation", "example");
/// // Perform work...
/// </code>
/// </example>
public abstract class ActivitySourceBase : IDisposable {
    /// <summary>
    /// ActivitySource
    /// </summary>
    public const string ConfigurationSectionName = "ActivitySource";

    private static readonly Lock _LockDictInstanceByType = new();
    private static Dictionary<Type, ActivitySourceBase> _DictInstanceByType = [];

    protected static T GetInstanceByType<T>()
        where T : ActivitySourceBase, new() {
        Type type = typeof(T);
        if (_DictInstanceByType.TryGetValue(type, out var instance)) {
            if (instance is T result) {
                return result;
            } else {
                throw new InvalidOperationException("InstanceByType");
            }
        } else {
            return new();
        }
    }

    internal static bool TryGetInstanceByType(Type type, [MaybeNullWhen(false)] out ActivitySourceBase activitySourceBase) {
        if (_DictInstanceByType.TryGetValue(type, out var instance)) {
            activitySourceBase = instance;
            return true;
        } else {
            activitySourceBase = default;
            return false;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivitySourceBase"/> class with the specified configuration and source information.
    /// </summary>
    /// <param name="configuration">
    /// The configuration instance used to read activity source settings. If provided, the activity source will
    /// automatically monitor configuration changes and update log levels accordingly. Can be null.
    /// </param>
    /// <param name="sourceName">
    /// The name of the activity source. This name is used to identify the source in tracing systems
    /// and must not be null or empty.
    /// </param>
    /// <param name="sourceVersion">
    /// The version of the activity source. This is optional and can be used for versioning in tracing systems.
    /// Defaults to null if not specified.
    /// </param>
    /// <param name="logLevel">
    /// The initial log level for the activity source. If not specified, defaults to <see cref="LogLevel.Information"/>.
    /// This level determines which activities will be created based on their requested log level.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceName"/> is null or empty.</exception>
    /// <remarks>
    /// <para>
    /// If a configuration is provided, the constructor sets up automatic monitoring of the "ActivitySource"
    /// configuration section. When this section changes, the log level for this activity source will be
    /// automatically updated based on the configuration value for the source name.
    /// </para>
    /// </remarks>
    protected ActivitySourceBase(
        IConfiguration? configuration,
        string sourceName,
        string? sourceVersion = default,
        LogLevel? logLevel = default
        ) {
        if (string.IsNullOrEmpty(sourceName)) { throw new ArgumentNullException(nameof(sourceName)); }
        this.SourceName = sourceName;
        this.SourceVersion = sourceVersion;
        this._ActivitySource = new ActivitySource(this.SourceName);
        if (logLevel is { } logLevelValue) {
            if (LogLevel.Trace <= logLevel && logLevel <= LogLevel.None) {
                this._LogLevel = logLevelValue;
            } else {
                throw new ArgumentException($"{logLevel} is invalid.", nameof(logLevel));
            }
        }
        if (configuration is not null) {
            ChangeToken.OnChange(
                () => configuration.GetSection(ConfigurationSectionName).GetReloadToken(),
                (sectionActivitySource) => this.SetLogLevel(sectionActivitySource),
                configuration.GetSection(ConfigurationSectionName)
                );
            this.SetLogLevel(configuration.GetSection(ConfigurationSectionName));
        }

        using (var scope = _LockDictInstanceByType.EnterScope()) {
            var next = new Dictionary<Type, ActivitySourceBase>(_DictInstanceByType);
            var type = this.GetType();
            next[type] = this;
            _DictInstanceByType = next;
        }
    }

    /// <summary>
    /// Sets the log level for this activity source based on a configuration section.
    /// </summary>
    /// <param name="sectionActivitySource">
    /// The configuration section containing activity source settings. The method will look for
    /// a value with the key matching this activity source's name.
    /// </param>
    /// <remarks>
    /// This method is typically called automatically when configuration changes are detected.
    /// It reads the configuration value for this activity source's name and calls <see cref="SetLogLevel(string)"/>
    /// to parse and apply the log level.
    /// </remarks>
    private void SetLogLevel(IConfigurationSection sectionActivitySource) {
        if (sectionActivitySource.Exists()) {
            if (sectionActivitySource.GetValue<string>(this.SourceName) is { Length: > 0 } value1) {
                this.SetLogLevel(value1);
                return;
            }
            if (sectionActivitySource.GetValue<string>("Default") is { Length: > 0 } value2) {
                this.SetLogLevel(value2);
                return;
            }
        }
    }

    /// <summary>
    /// The current log level for this activity source.
    /// </summary>
    protected LogLevel _LogLevel = LogLevel.Trace;

    /// <summary>
    /// The underlying .NET ActivitySource instance.
    /// </summary>
    protected ActivitySource? _ActivitySource;

    /// <summary>
    /// Gets the underlying .NET <see cref="ActivitySource"/> instance used for creating activities.
    /// </summary>
    /// <value>The ActivitySource instance that was created during construction.</value>
    public ActivitySource ActivitySource => this._ActivitySource
        ?? throw new ObjectDisposedException(nameof(this.ActivitySource));

    /// <summary>
    /// Gets the name of this activity source.
    /// </summary>
    /// <value>The source name that was provided during construction.</value>
    public string SourceName { get; }

    /// <summary>
    /// Gets the version of this activity source, if specified.
    /// </summary>
    /// <value>The source version that was provided during construction, or null if not specified.</value>
    public string? SourceVersion { get; }

    /// <summary>
    /// Sets the log level for this activity source.
    /// </summary>
    /// <param name="level">The log level to set. This determines which activities will be created based on their requested log level.</param>
    public bool SetLogLevel(LogLevel level) {
        if (LogLevel.Trace <= level && level <= LogLevel.None) {
            this._LogLevel = level;
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Gets the current log level for this activity source.
    /// </summary>
    /// <value>The current log level that determines which activities will be created.</value>
    public LogLevel LogLevel => this._LogLevel;

    /// <summary>
    /// Sets the log level for this activity source by parsing a string value.
    /// </summary>
    public bool SetLogLevel(string? value) {
        if (LogLevelUtility.TryGetLogLevelByName(value, out var logLevel)) {
            this._LogLevel = logLevel;
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Determines whether activities with the specified log level should be created.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>
    /// <c>true</c> if activities with the specified log level should be created; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs several checks to determine if an activity should be created:
    /// </para>
    /// <list type="number">
    /// <item><description>Checks if the underlying ActivitySource has any listeners</description></item>
    /// <item><description>Verifies that neither the requested log level nor the current log level is None</description></item>
    /// <item><description>Compares the requested log level against the current minimum log level</description></item>
    /// </list>
    /// <para>
    /// Activities are only created if there are listeners and the requested log level meets or exceeds
    /// the minimum log level configured for this activity source.
    /// </para>
    /// </remarks>
    public bool IsEnabled(LogLevel logLevel) {
        if (this._ActivitySource is not { }) { return false; }

        return
            (LogLevel.Trace <= logLevel && logLevel < LogLevel.None)
            && (this._LogLevel <= logLevel);
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

    /// <summary>
    /// Creates and starts a new root <see cref="Activity"/> that temporarily replaces the current activity context.
    /// </summary>
    /// <param name="kind">The <see cref="ActivityKind"/> for the new activity.</param>
    /// <param name="parentContext">The parent <see cref="ActivityContext"/> object to initialize the created Activity object with.</param>
    /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
    /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
    /// <param name="startTime">The optional start timestamp to set on the created Activity object.</param>
    /// <param name="logLevel">The optional log level. Default is Information.</param>
    /// <param name="name">The operation name of the Activity. If not provided, the calling method name is used.</param>
    /// <returns>
    /// A <see cref="RestoreRootActivity"/> instance that manages the new root activity and context restoration,
    /// or null if there are no listeners or the log level is not enabled.
    /// </returns>
    /// <example>
    /// <code>
    /// using(var rootActivity = activitySource.StartRootActivity()) {
    ///     rootActivity?.Activity?.SetTag("operation", "root-operation");
    ///     // Perform work in the root activity context...
    /// }
    /// // Activity is automatically stopped and previous context restored when disposed
    /// </code>
    /// </example>
    public RestoreRootActivity StartRootActivity(
        ActivityTraceId? activityTraceId = default,
        ActivityKind kind = ActivityKind.Internal,
        ActivityContext parentContext = default,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null,
        DateTimeOffset startTime = default,
        LogLevel logLevel = LogLevel.Information,
        [CallerMemberName] string name = "") {
        if (this.IsEnabled(logLevel)) {
            var previous = Activity.Current;
            ActivityTraceId traceId;
            if (previous is { }) {
                Activity.Current = null;
                if (parentContext is { }) {
                    traceId = parentContext.TraceId;
                } else {
                    traceId = ActivityTraceId.CreateRandom();
                }
            } else {
                if (activityTraceId is { } activityTraceIdValue) {
                    traceId = activityTraceIdValue;
                } else {
                    traceId = ActivityTraceId.CreateRandom();
                }
            }
            var rootContext = new ActivityContext(
                    traceId,
                    ActivitySpanId.CreateRandom(),
                    ActivityTraceFlags.Recorded);

            var rootActivity = this.ActivitySource.StartActivity(
                kind, rootContext, tags, links, startTime, name);

            if (rootActivity is { }) {
                Activity.Current = rootActivity;
                RestoreRootActivity result = new(previous, rootActivity);
                return result;
            } else {
                return new(default, default);
            }
        } else {
            return new(default, default);
        }
    }

    protected virtual void Dispose(bool disposing) {
        using (var activitySource = this._ActivitySource) {
            this._ActivitySource = null;

            using (var scope = _LockDictInstanceByType.EnterScope()) {
                var next = new Dictionary<Type, ActivitySourceBase>(_DictInstanceByType);
                var type = this.GetType();
                next.Remove(type);
                _DictInstanceByType = next;
            }
        }
    }

    ~ActivitySourceBase() {
        this.Dispose(disposing: false);
    }

    public void Dispose() {
        this.Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
}

#pragma warning disable IDE0009 // Member access should be qualified.
/// <summary>
/// A disposable struct that manages a root activity and automatically restores the previous activity context when disposed.
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// using(var rootActivity = activitySource.StartRootActivity()){
///   // Work is performed in the root activity context
///   rootActivity?.Activity?.SetTag("operation", "some-operation");
///
/// }
/// </code>
/// </example>
public struct RestoreRootActivity : IDisposable {
    /// <summary>
    /// The activity that was current before this root activity was started.
    /// This will be restored when the struct is disposed.
    /// </summary>
    private readonly Activity? _PreviousActivity;

    /// <summary>
    /// The root activity being managed by this instance.
    /// </summary>
    private Activity? _RootActivity;

    /// <summary>
    /// Gets the root activity managed by this instance.
    /// </summary>
    public readonly Activity? Activity => this._RootActivity;

    /// <summary>
    /// Donot use this constructor. Use <see cref="ActivitySourceBase.StartRootActivity"/> instead.
    /// </summary>
    public RestoreRootActivity(Activity? previous, Activity? rootActivity) {
        _PreviousActivity = previous;
        _RootActivity = rootActivity;
    }

    /// <summary>
    /// Disposes the root activity and restores the previous activity context.
    /// </summary>
    /// <remarks>
    /// The previous activity is only restored if it exists and has not been stopped,
    /// preventing the restoration of invalid activity contexts.
    /// </para>
    /// </remarks>
    public void Dispose() {
        try {
            using (var rootActivity = _RootActivity) {
                _RootActivity = null;
                Activity.Current = null;
                rootActivity?.Stop();
            }
            if (_PreviousActivity is not null && !_PreviousActivity.IsStopped) {
                Activity.Current = _PreviousActivity;
            }
        } catch {
        }
    }
}
#pragma warning restore IDE0009 // Member access should be qualified.