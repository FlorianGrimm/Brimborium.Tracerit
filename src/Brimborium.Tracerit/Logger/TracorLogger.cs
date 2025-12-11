namespace Brimborium.Tracerit.Logger;

internal sealed class TracorLogger : ILogger {
    public const string OwnNamespace = "Brimborium.Tracerit";
    public const int OwnNamespaceLength = /* OwnNamespace.Length */ 19;

    private readonly string? _Name;
    private readonly ITracorDataConvertService _TracorDataConvertService;
    private readonly ITracorCollectivePublisher _Publisher;
    private readonly LogLevel? _MinimumLogLevel;
    private readonly IExternalScopeProvider? _ExternalScopeProvider;
    private readonly TracorIdentifier _Id;
    private readonly bool _IsAllowed;
    private readonly TracorDataRecordPool _Pool;
    private readonly TracorIdentifierCache _IdChildCache;

    public TracorLogger(
        string applicationName,
        string name,
        TracorDataRecordPool tracorDataRecordPool,
        ITracorDataConvertService tracorDataConvertService,
        ITracorCollectivePublisher publisher,
        LogLevel? minimumLogLevel,
        IExternalScopeProvider? externalScopeProvider) {
        this._Name = name;
        this._TracorDataConvertService = tracorDataConvertService;
        this._Publisher = publisher;
        this._Pool = tracorDataRecordPool;

        // if the name is this namespace or sub-namespace - don't tracor
        if (publisher.IsEnabled()) {
            if (name is { Length: >= OwnNamespaceLength }
                && name.StartsWith(OwnNamespace)) {
                if (OwnNamespaceLength < name.Length) {
                    this._IsAllowed = !('.' == name[OwnNamespaceLength]);
                } else {
                    this._IsAllowed = false;
                }
            } else {
                this._IsAllowed = true;
            }
        } else {
            this._IsAllowed = false;
        }
        this._MinimumLogLevel = minimumLogLevel;
        this._ExternalScopeProvider = externalScopeProvider;
        
        string nameNormalized = (name is { Length: > 0 }) ? name : string.Empty;
        this._Id = new TracorIdentifier(applicationName, "Logger", nameNormalized, string.Empty);
        this._IdChildCache = new TracorIdentifierCache(this._Id);
        //if (name is { Length: > 0 }) {
        //    this._Id = new TracorIdentifier(string.Empty, "Logger", name, string.Empty);
        //} else {
        //    this._Id = new TracorIdentifier(string.Empty, "Logger", "", string.Empty);
        //}
        //this._IdChildCache = new TracorIdentifierCache(this._Id);
    }

    public bool IsEnabled(LogLevel logLevel) {
        if (this._IsAllowed && this._Publisher.IsEnabled()) {
            if (this._MinimumLogLevel is { } minimumLogLevel) {
                return minimumLogLevel <= logLevel;
            } else {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        return this._ExternalScopeProvider?.Push(state);

        // https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.EventSource/src/EventSourceLogger.cs
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        try {
            if (!this._IsAllowed) {
                return;
            }

            if (SuppressInstrumentationScope.IncrementIfTriggered() == 0) {
                try {
                    DateTime utcNow = DateTime.UtcNow;

                    var activity = Activity.Current;
                    string activityTraceId;
                    string activitySpanId;
                    string activityTraceFlags;
                    if (activity != null) {
                        if (activity.IdFormat == ActivityIdFormat.W3C) {
                            activityTraceId = activity.TraceId.ToHexString();
                            activitySpanId = activity.SpanId.ToHexString();
                            activityTraceFlags = activity.ActivityTraceFlags == ActivityTraceFlags.None ? "0" : "1";
                        } else {
                            activityTraceId = activity.TraceId.ToHexString();
                            activitySpanId = activity.SpanId.ToHexString();
                            activityTraceFlags = activity.ActivityTraceFlags == ActivityTraceFlags.None ? "0" : "1";
                        }
                    } else {
                        activityTraceId = string.Empty;
                        activitySpanId = string.Empty;
                        activityTraceFlags = string.Empty;
                    }

                    string formatted = formatter(state, exception);
                    using (TracorDataRecord loggerTracorData = this._Pool.Rent()) {
                        this.ConvertProperties(
                            loggerTracorData,
                            this._IdChildCache.Child(eventId.ToString()),
                            activityTraceId,
                            activitySpanId,
                            activityTraceFlags,
                            logLevel,
                            eventId,
                            state,
                            formatted,
                            exception);
                        loggerTracorData.Timestamp = utcNow;
                        this._Publisher.OnTrace(true, loggerTracorData);
                    }
                } finally {
                    SuppressInstrumentationScope.DecrementIfTriggered();
                }
            }
        } catch {
        }
    }
    private static ExceptionInfo GetExceptionInfo(Exception? exception) {
        return exception != null ? new ExceptionInfo(exception) : ExceptionInfo.Empty;
    }

    private void ConvertProperties(
        TracorDataRecord loggerTracorData,
        TracorIdentifier tracorIdentifier,
        string activityTraceId,
        string activitySpanId,
        string activityTraceFlags,
        LogLevel logLevel,
        EventId eventId,
        object? state,
        string formatted,
        Exception? exception) {
        // TODO: key from otel
        loggerTracorData.TracorIdentifier = new(tracorIdentifier.RescourceName, tracorIdentifier.SourceProvider, tracorIdentifier.Scope, formatted);

        if (activityTraceId is { Length: > 0 }) {
            loggerTracorData.ListProperty.Add(
                new TracorDataProperty(
                    TracorConstants.TracorDataPropertyNameActivityTraceId,
                    activityTraceId));
        }
        if (activitySpanId is { Length: > 0 }) {
            loggerTracorData.ListProperty.Add(
                new TracorDataProperty(
                    TracorConstants.TracorDataPropertyNameActivitySpanId,
                    activitySpanId));
        }
        if (activityTraceFlags is { Length: > 0 }) {
            loggerTracorData.ListProperty.Add(
                new TracorDataProperty(
                    TracorConstants.TracorDataPropertyNameActivityTraceFlags,
                    activityTraceFlags));
        }
        loggerTracorData.ListProperty.Add(
            new TracorDataProperty(
                TracorConstants.TracorDataPropertyNameLogLevel,
                (LogLevel.Trace <= logLevel && logLevel <= LogLevel.None) ? logLevel : LogLevel.None));
        if (eventId.Id != 0) {
            loggerTracorData.ListProperty.Add(
                new TracorDataProperty(
                    TracorConstants.TracorDataPropertyNameEventId,
                    eventId.Id));
        }
        if (eventId.Name is { Length: > 0 }) {
            loggerTracorData.ListProperty.Add(
                new TracorDataProperty(
                    TracorConstants.TracorDataPropertyNameEventName,
                    eventId.Name));
        }
        //if (formatted is { Length: > 0 }) {
        //    loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("Message", formatted));
        //}
        if (exception is { }) {
            var exceptionInfo = GetExceptionInfo(exception);
            loggerTracorData.ListProperty.Add(
                new TracorDataProperty(
                    TracorConstants.TracorDataPropertyNameExceptionTypeName,
                    exceptionInfo.TypeName ?? string.Empty));
            loggerTracorData.ListProperty.Add(
                new TracorDataProperty(
                    TracorConstants.TracorDataPropertyNameExceptionMessage,
                    exceptionInfo.Message ?? string.Empty));
            loggerTracorData.ListProperty.Add(
                new TracorDataProperty(
                    TracorConstants.TracorDataPropertyNameExceptionHResult,
                    exceptionInfo.HResult.ToString()));
            loggerTracorData.ListProperty.Add(
                new TracorDataProperty(
                    TracorConstants.TracorDataPropertyNameExceptionVerboseMessage,
                    exceptionInfo.VerboseMessage ?? string.Empty));
        }
        if (state is IReadOnlyList<KeyValuePair<string, object?>> keyValuePairs) {
            List<TracorDataProperty> listProperty = loggerTracorData.ListProperty;
            foreach (var (key, value) in keyValuePairs) {
                if (value is null) { continue; }

                this._TracorDataConvertService.ConvertObjectToListProperty(
                    isPublic: true,
                    levelWatchDog: 1,
                    key, value,
                    listProperty);
            }
        }
    }
}