using Brimborium.Tracerit.Diagnostics;

namespace System.Diagnostics;

public static class ActivitySourceExtensíon {

    public static RestoreRootActivity StartRootActivity(
        this ActivitySource? that,
        ActivityKind kind = ActivityKind.Internal,
        ActivityContext parentContext = default,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null,
        DateTimeOffset startTime = default,
        [CallerMemberName] string name = "",
        ActivityTraceId? activityTraceId = default
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
}
