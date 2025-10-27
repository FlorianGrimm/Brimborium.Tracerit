#pragma warning disable IDE0130 // Namespace does not match folder structure



namespace Brimborium.Tracerit.DataAccessor;

public sealed class ActivityTracorDataFactory
    : ITracorDataAccessorFactory<Activity> {
    private readonly TracorDataRecordPool _TracorDataRecordPool;

    public ActivityTracorDataFactory(
        TracorDataRecordPool tracorDataRecordPool) {
        this._TracorDataRecordPool = tracorDataRecordPool;
    }

    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is Activity activity) {
            // tracorData = new ActivityTracorData(activity);
            var result = this._TracorDataRecordPool.Rent();
            this.SetActivityProperties(activity, result);
            tracorData = result;
            return true;
        }
        tracorData = null;
        return false;
    }

    public bool TryGetDataTyped(Activity value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this._TracorDataRecordPool.Rent();
        this.SetActivityProperties(value, result);
        tracorData = result;
        return true;
    }

    private const string PrefixTag = "tag.";

    private void SetActivityProperties(Activity activity, TracorDataRecord target) {
        {
            target.ListProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivitySpanId,
                    activity.Id ?? string.Empty));

            target.ListProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivityTraceId,
                    activity.TraceId.ToString()));

            if (activity.ParentId is { Length: > 0 } parentId) { 
                target.ListProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivityParentTraceId,
                    parentId ?? string.Empty));
            }

            var parentSpanId = activity.ParentSpanId;
            if ("0000000000000000" != parentSpanId.ToHexString()) {
                target.ListProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivityParentSpanId,
                    parentSpanId.ToHexString()));
            }

            target.ListProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameOperationName,
                    activity.OperationName));

            if (!ReferenceEquals(activity.OperationName, activity.DisplayName)) {
                target.ListProperty.Add(
                    TracorDataProperty.CreateStringValue(
                        TracorConstants.TracorDataPropertyNameDisplayName,
                        activity.DisplayName));
            }

            target.ListProperty.Add(
                TracorDataProperty.CreateDateTimeValue(
                    TracorConstants.TracorDataPropertyNameStartTimeUtc,
                    activity.StartTimeUtc));

            target.ListProperty.Add(
                TracorDataProperty.CreateDateTimeValue(
                    TracorConstants.TracorDataPropertyNameStopTimeUtc,
                    activity.StartTimeUtc.Add(activity.Duration)));
        }
        {
            var enumeratorTagObjects = activity.EnumerateTagObjects();

            if (enumeratorTagObjects.MoveNext()) {
                ref readonly var tag = ref enumeratorTagObjects.Current;
                if (tag.Value is { } tagValue) {
                    target.ListProperty.Add(TracorDataProperty.Create(PrefixTag + tag.Key, tagValue));
                }
            }
        }

        // TODO:
        // value.EnumerateLinks
        // value.EnumerateEvents
    }
}
#if false

public sealed class ActivityTracorDataFactory
    : ITracorDataAccessorFactory<Activity> {
    private readonly ActivityTracorDataPool _ActivityTracorDataPool;

    public ActivityTracorDataFactory(
        ActivityTracorDataPool activityTracorDataPool) {
        this._ActivityTracorDataPool = activityTracorDataPool;
    }

    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is Activity activity) {
            // tracorData = new ActivityTracorData(activity);
            var result = this._ActivityTracorDataPool.Rent();
            result.SetValue(activity);
            tracorData = result;
            return true;
        }
        tracorData = null;
        return false;
    }

    public bool TryGetDataTyped(Activity value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this._ActivityTracorDataPool.Rent();
        result.SetValue(value);
        tracorData = result;
        return true;
    }
}
#endif