namespace System.Diagnostics;

/// <summary>
/// The Activity has strange overloads for StartActivity so let's add this for better confusion.
/// Only one overload, all parameter with default so you can use named parameters.
/// </summary>
public static class TracorActivitySourceExtension {
    public static RestoreRootActivity StartRoot(
        this InstrumentationBase? that,
        [CallerMemberName] string name = "",
        ActivityKind kind = ActivityKind.Internal,
        ActivityContext parentContext = default,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null,
        ActivityTraceId? activityTraceId = default,
        DateTimeOffset startTime = default
        ) {
        if (that is { ActivitySource: { } activitySource }) {
            return StartRoot(activitySource, name, kind, parentContext, tags, links, activityTraceId, startTime);
        } else {
            return new(null, null);
        }
    }

    public static RestoreRootActivity StartRoot(
        this ActivitySource? that,
        [CallerMemberName] string name = "",
        ActivityKind kind = ActivityKind.Internal,
        ActivityContext parentContext = default,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null,
        ActivityTraceId? activityTraceId = default,
        DateTimeOffset startTime = default
        ) {
        if (that is null) {
            return new(default, default);
        }
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

        var rootActivity = that.StartActivity(kind, rootContext, tags, links, startTime, name);

        if (rootActivity is not { }) {
            return new(default, default);
        }

        {
            Activity.Current = rootActivity;
            RestoreRootActivity result = new(previous, rootActivity);
            return result;
        }
    }


    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.
    /// </summary>
    /// <param name="name">The operation name of the Activity.</param>
    /// <param name="kind">The <see cref="ActivityKind"/></param>
    /// <param name="parentContext">The parent <see cref="ActivityContext"/> object to initialize the created Activity object with.</param>
    /// <param name="parentId">The parent Id to initialize the created Activity object with.</param>
    /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
    /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
    /// <param name="startTime">The optional start timestamp to set on the created Activity object.</param>
    /// <returns>The created <see cref="Activity"/> object or null if there is no any listener.</returns>
    public static Activity? Start(
        this InstrumentationBase? that,
        [CallerMemberName] string name = "",
        ActivityKind kind = ActivityKind.Internal,
        string? parentId = default,
        ActivityContext parentContext = default,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null,
        DateTimeOffset startTime = default) {
        if (that is { ActivitySource: { } activitySource }) {
            if (parentContext is { }) {
                return activitySource.StartActivity(kind, parentContext, tags, links, startTime, name);
            } else { 
                return activitySource.StartActivity(name, kind, parentId, tags, links, startTime);
            }
        } else {
            return null;
        }
    }

    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.
    /// </summary>
    /// <param name="name">The operation name of the Activity.</param>
    /// <param name="kind">The <see cref="ActivityKind"/></param>
    /// <param name="parentContext">The parent <see cref="ActivityContext"/> object to initialize the created Activity object with.</param>
    /// <param name="parentId">The parent Id to initialize the created Activity object with.</param>
    /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
    /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
    /// <param name="startTime">The optional start timestamp to set on the created Activity object.</param>
    /// <returns>The created <see cref="Activity"/> object or null if there is no any listener.</returns>
    public static Activity? Start(
        this ActivitySource? that,
        [CallerMemberName] string name = "",
        ActivityKind kind = ActivityKind.Internal,
        string? parentId = default,
        ActivityContext parentContext = default,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null,
        DateTimeOffset startTime = default) {
        if (that is { } activitySource) {
            if (parentContext is { }) {
                return activitySource.StartActivity(kind, parentContext, tags, links, startTime, name);
            } else {
                return activitySource.StartActivity(name, kind, parentId, tags, links, startTime);
            }
        } else {
            return null;
        }
    }
}
