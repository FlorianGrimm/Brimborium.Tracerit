using System.Runtime.CompilerServices;

namespace Brimborium.Tracerit.Logger;

internal sealed class TracorLogger : ILogger {
    internal const string OwnNamespace = "Brimborium.Tracerit";
    internal const int OwnNamespaceLength = /* OwnNamespace.Length */ 19;

    private readonly string? _Name;
    private readonly ITracor _Tracor;
    private readonly IExternalScopeProvider? _ExternalScopeProvider;
    private readonly TracorIdentitfier _Id;
    private readonly bool _IsAllowed;
    private readonly LoggerTracorDataPool _Pool;

    public TracorLogger(string? name, ITracor tracor, IExternalScopeProvider? externalScopeProvider) {
        this._Name = name;
        this._Pool = new(2048);

        // if the name is this namespace or sub-namespace - don't tracor
        if (tracor.IsGeneralEnabled()) {
            if (name is { Length: >= OwnNamespaceLength }
                && name.StartsWith(OwnNamespace)) {
                if (name.Length == OwnNamespaceLength) {
                    this._IsAllowed = false;
                } else {
                    this._IsAllowed = '.' != name[OwnNamespaceLength];
                }
            } else {
                this._IsAllowed = true;
            }
        } else {
            this._IsAllowed = false;
        }
        this._Tracor = tracor;

        this._ExternalScopeProvider = externalScopeProvider;
        if (name is { Length: > 0 }) {
            this._Id = new TracorIdentitfier(name);
        } else {
            this._Id = new TracorIdentitfier("::");
        }
    }

    public bool IsEnabled(LogLevel logLevel) {
        return this._IsAllowed && this._Tracor.IsCurrentlyEnabled();
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        return this._ExternalScopeProvider?.Push(state);

        // https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.EventSource/src/EventSourceLogger.cs
        //IReadOnlyList<KeyValuePair<string, string?>> arguments = GetProperties(state);
        //_eventSource.ActivityStart(id, _factoryID, CategoryName, arguments);
        //return new ActivityScope(_eventSource, CategoryName, id, _factoryID, false);
    }

    //private sealed class ActivityScope : IDisposable {
    //    private readonly string _categoryName;
    //    private readonly int _activityID;
    //    private readonly int _factoryID;
    //    private readonly bool _isJsonStop;
    //    private readonly LoggingEventSource _eventSource;

    //    public ActivityScope(LoggingEventSource eventSource, string categoryName, int activityID, int factoryID, bool isJsonStop) {
    //        _categoryName = categoryName;
    //        _activityID = activityID;
    //        _factoryID = factoryID;
    //        _isJsonStop = isJsonStop;
    //        _eventSource = eventSource;
    //    }

    //    public void Dispose() {
    //        if (_isJsonStop) {
    //            _eventSource.ActivityJsonStop(_activityID, _factoryID, _categoryName);
    //        } else {
    //            _eventSource.ActivityStop(_activityID, _factoryID, _categoryName);
    //        }
    //    }
    //}


    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (!(this._IsAllowed && this._Tracor.IsCurrentlyEnabled())) {
            return;
        }

        var activity = Activity.Current;
        string activityTraceId;
        string activitySpanId;
        string activityTraceFlags;
        if (activity != null && activity.IdFormat == ActivityIdFormat.W3C) {
            activityTraceId = activity.TraceId.ToHexString();
            activitySpanId = activity.SpanId.ToHexString();
            activityTraceFlags = activity.ActivityTraceFlags == ActivityTraceFlags.None ? "0" : "1";
        } else {
            activityTraceId = string.Empty;
            activitySpanId = string.Empty;
            activityTraceFlags = string.Empty;
        }

        //string formatted = formatter(state, exception);
        using (LoggerTracorData loggerTracorData = this._Pool.Rent()) {
            ConvertProperties(
                loggerTracorData,
                this._Id,
                activityTraceId,
                activitySpanId,
                activityTraceFlags,
                eventId,
                state,
                exception);
            this._Tracor.Trace(
                new TracorIdentitfier(this._Id, eventId.ToString()),
                loggerTracorData
                );
        }
    }
    private static ExceptionInfo GetExceptionInfo(Exception? exception) {
        return exception != null ? new ExceptionInfo(exception) : ExceptionInfo.Empty;
    }
    private static void ConvertProperties(
        LoggerTracorData loggerTracorData,
        TracorIdentitfier id,
        string activityTraceId,
        string activitySpanId,
        string activityTraceFlags,
        EventId eventId,
        object? state,
        Exception? exception) {
        // TODO: key from otel
        loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("Source", id.Callee));
        loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("Activity.TraceId", activityTraceId));
        loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("Activity.SpanId", activitySpanId));
        loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("Activity.TraceFlags", activityTraceFlags));
        loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("Event.Id", eventId.Id));
        loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("Event.Name", eventId.Name));
        if (exception is { }) {
            var exceptionInfo = GetExceptionInfo(exception);
            loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("TypeName", exceptionInfo.TypeName));
            loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("Message", exceptionInfo.Message));
            loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("HResult", exceptionInfo.HResult.ToString()));
            loggerTracorData.Arguments.Add(new KeyValuePair<string, object?>("VerboseMessage", exceptionInfo.VerboseMessage));
        }
        if (state is IReadOnlyList<KeyValuePair<string, object?>> keyValuePairs) {
            loggerTracorData.Arguments.AddRange(keyValuePairs);
        }
    }
}