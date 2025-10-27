#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Brimborium.Tracerit.DataAccessor;

public sealed class ActivityTracorData
    : ReferenceCountObject<Activity>
    , ITracorData<Activity> {
    public ActivityTracorData() : base(default) { }

    public ActivityTracorData(IReferenceCountPool? owner) : base(owner) { }

    public ActivityTracorData(Activity activity) : base(default) {
        this._Value = activity;
    }

    private const string PrefixTag = "tag.";
    public List<string> GetListPropertyName() {
        var value = this.GetValue();
        List<string> result = new();
        foreach (ref readonly var tag in value.EnumerateTagObjects()) {
            //result.Add(tag.Key);
            result.Add($"{PrefixTag}{tag.Key}");
        }

        return result;
    }

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out Activity value) {
        value = this.GetValue();
        return true;
    }

    public object? this[string propertyName] => this.TryGetPropertyValue(propertyName, out var value) ? value : null;

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        var value = this.GetValue();

        if (TracorConstants.TracorDataPropertyNameActivitySpanId == propertyName) {
            propertyValue = value.Id ?? string.Empty;
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameActivityTraceId == propertyName) {
            propertyValue = value.TraceId.ToString();
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameOperationName == propertyName) {
            propertyValue = value.OperationName;
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameDisplayName == propertyName) {
            propertyValue = value.DisplayName;
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameStartTimeUtc == propertyName) {
            propertyValue = value.StartTimeUtc;
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameStopTimeUtc == propertyName) {
            propertyValue = value.StartTimeUtc.Add(value.Duration);
            return true;
        }

        if (propertyName.StartsWith(PrefixTag)) {
            var tagName = propertyName.Substring(PrefixTag.Length);

            foreach (ref readonly var tag in value.EnumerateTagObjects()) {
                if (tagName == tag.Key) {
                    propertyValue = tag.Value;
                    return true;
                }
            }
        }

        // TODO:
        // value.EnumerateLinks
        // value.EnumerateEvents

        propertyValue = null;
        return false;
    }

    /// <summary>
    /// Gets the value of a specific tag on an <see cref="Activity"/>.
    /// </summary>
    /// <param name="activity">Activity instance.</param>
    /// <param name="tagName">Case-sensitive tag name to retrieve.</param>
    /// <returns>Tag value or null if a match was not found.</returns>
    public object? GetTagValue(string? tagName) {
        if (tagName is not { Length: > 0 }) {
            return null;
        }

        var value = this.GetValue();

        if (tagName.StartsWith(PrefixTag)) {
            tagName = tagName.Substring(PrefixTag.Length);
        }
        foreach (ref readonly var tag in value.EnumerateTagObjects()) {
            if (tag.Key == tagName) {
                return tag.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if the user provided tag name is the first tag of the <see cref="Activity"/> and retrieves the tag value.
    /// </summary>
    /// <param name="activity">Activity instance.</param>
    /// <param name="tagName">Tag name.</param>
    /// <param name="tagValue">Tag value.</param>
    /// <returns><see langword="true"/> if the first tag of the supplied Activity matches the user provide tag name.</returns>
    public bool TryGetTagValue(string tagName, out object? tagValue) {
        var value = this.GetValue();
        if (tagName.StartsWith(PrefixTag)) {
            tagName = tagName.Substring(PrefixTag.Length);
        }

        var enumeratorTagObjects = value.EnumerateTagObjects();

        if (enumeratorTagObjects.MoveNext()) {
            ref readonly var tag = ref enumeratorTagObjects.Current;

            if (tag.Key == tagName) {
                tagValue = tag.Value;
                return true;
            }
        }

        tagValue = null;
        return false;
    }

    public bool TryGetTagValue<T>(string tagName, [MaybeNullWhen(false)] out T tagValue) {
        var value = this.GetValue();
        if (tagName.StartsWith(PrefixTag)) {
            tagName = tagName.Substring(PrefixTag.Length);
        }

        var enumeratorTagObjects = value.EnumerateTagObjects();

        if (enumeratorTagObjects.MoveNext()) {
            ref readonly var tag = ref enumeratorTagObjects.Current;

            if (tag.Key == tagName && tag.Value is T tValue) {
                tagValue = tValue;
                return true;
            }
        }

        tagValue = default;
        return false;
    }

    /// <summary>
    /// Gets or sets the identifier associated with this trace data record.
    /// </summary>
    public TracorIdentifier TracorIdentifier { get; set; }

    public DateTime Timestamp { get; set; }


    public bool TryGetDataProperty(string propertyName, out TracorDataProperty result) {
        var value = this.GetValue();

        if (TracorConstants.TracorDataPropertyNameActivitySpanId == propertyName) {
            result = TracorDataProperty.CreateStringValue(
                TracorConstants.TracorDataPropertyNameActivitySpanId,
                value.Id ?? string.Empty);
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameActivityTraceId == propertyName) {
            result = TracorDataProperty.CreateStringValue(
                TracorConstants.TracorDataPropertyNameActivityTraceId,
                value.TraceId.ToString());
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameOperationName == propertyName) {
            result = TracorDataProperty.CreateStringValue(
                TracorConstants.TracorDataPropertyNameOperationName,
                value.OperationName);
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameDisplayName == propertyName) {
            result = TracorDataProperty.CreateStringValue(
                TracorConstants.TracorDataPropertyNameDisplayName,
                value.DisplayName);
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameStartTimeUtc == propertyName) {
            result = TracorDataProperty.CreateDateTimeValue(
                TracorConstants.TracorDataPropertyNameStartTimeUtc,
                value.StartTimeUtc);
            return true;
        }
        if (TracorConstants.TracorDataPropertyNameStopTimeUtc == propertyName) {
            result = TracorDataProperty.CreateDateTimeValue(
                TracorConstants.TracorDataPropertyNameStopTimeUtc,
                value.StartTimeUtc.Add(value.Duration));
            return true;
        }

        if (propertyName.StartsWith(PrefixTag)) {
            var tagName = propertyName.Substring(PrefixTag.Length);

            var enumeratorTagObjects = value.EnumerateTagObjects();

            if (enumeratorTagObjects.MoveNext()) {
                ref readonly var tag = ref enumeratorTagObjects.Current;

                if (string.Equals(tag.Key, tagName, StringComparison.Ordinal)) {
                    result = TracorDataProperty.Create(propertyName, tag.Value);
                    return true;
                }
            }
        }

        result = new TracorDataProperty(string.Empty);
        return false;
    }

    public const string ActivityTracorDataId = "id";

    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        var value = this.GetValue();
        {
            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivitySpanId,
                    value.Id ?? string.Empty));

            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivityTraceId,
                    value.TraceId.ToString()));

            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameOperationName,
                    value.OperationName));

            if (!ReferenceEquals(value.OperationName, value.DisplayName)) {
                listProperty.Add(
                    TracorDataProperty.CreateStringValue(
                        TracorConstants.TracorDataPropertyNameDisplayName,
                        value.DisplayName));
            }

            listProperty.Add(
                TracorDataProperty.CreateDateTimeValue(
                    TracorConstants.TracorDataPropertyNameStartTimeUtc,
                    value.StartTimeUtc));

            listProperty.Add(
                TracorDataProperty.CreateDateTimeValue(
                    TracorConstants.TracorDataPropertyNameStopTimeUtc,
                    value.StartTimeUtc.Add(value.Duration)));
        }
        {
            var enumeratorTagObjects = value.EnumerateTagObjects();

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

    protected override void ResetState() {
        this._Value = null;
        this.TracorIdentifier = default;
    }

    protected override bool IsStateReset()
        => this._Value is null;
}