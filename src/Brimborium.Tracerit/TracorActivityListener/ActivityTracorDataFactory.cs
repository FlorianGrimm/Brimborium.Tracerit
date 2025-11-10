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
            SetActivityProperties(activity, result.ListProperty);
            tracorData = result;
            return true;
        }
        tracorData = null;
        return false;
    }

    public bool TryGetDataTyped(Activity value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this._TracorDataRecordPool.Rent();
        SetActivityProperties(value, result.ListProperty);
        tracorData = result;
        return true;
    }

#pragma warning disable IDE1006 // Naming Styles
    private const string PrefixTag = "tag.";
#pragma warning restore IDE1006 // Naming Styles

    private static void SetActivityProperties(Activity activity, List<TracorDataProperty> listProperty) {
        {
            TracorDataUtility.SetActivity(listProperty);

            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameOperationName,
                    activity.OperationName));

            if (!ReferenceEquals(activity.OperationName, activity.DisplayName)) {
                listProperty.Add(
                    TracorDataProperty.CreateStringValue(
                        TracorConstants.TracorDataPropertyNameDisplayName,
                        activity.DisplayName));
            }

            listProperty.Add(
                TracorDataProperty.CreateDateTimeValue(
                    TracorConstants.TracorDataPropertyNameStartTimeUtc,
                    activity.StartTimeUtc));

            listProperty.Add(
                TracorDataProperty.CreateDateTimeValue(
                    TracorConstants.TracorDataPropertyNameStopTimeUtc,
                    activity.StartTimeUtc.Add(activity.Duration)));
        }
        {
            var enumeratorTagObjects = activity.EnumerateTagObjects();

            if (enumeratorTagObjects.MoveNext()) {
                ref readonly var tag = ref enumeratorTagObjects.Current;
                if (tag.Value is { } tagValue) {
                    listProperty.Add(TracorDataProperty.Create(PrefixTag + tag.Key, tagValue));
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