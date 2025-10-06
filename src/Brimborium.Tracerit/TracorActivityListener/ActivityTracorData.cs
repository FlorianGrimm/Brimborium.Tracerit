#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.ComponentModel;

namespace Brimborium.Tracerit.DataAccessor;

public sealed class ActivityTracorData
    : ReferenceCountObject<Activity>
    , ITracorData<Activity> {
    public ActivityTracorData() : base(default) { }

    public ActivityTracorData(IReferenceCountPool? owner) : base(owner) { }

    public ActivityTracorData(Activity activity) : base(default) {
        this._Value = activity;
    }

    //public Activity Activity => this._Value ?? throw new ObjectDisposedException("ActivityTracorData");

    public List<string> GetListPropertyName() {
        var value = this.GetValue();
        List<string> result = new();
        foreach (ref readonly var tag in value.EnumerateTagObjects()) {
            result.Add(tag.Key);
            //result.Add($"{TracorDataUtility.PrefixTag}:{tag.Key}");
        }

        return result;
    }

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out Activity value) {
        value = this.GetValue();
        return true;
    }

    public object? this[string propertyName] => this.TryGetPropertyValue(propertyName, out var value) ? value : null;

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        /*
        foreach (ref readonly var activityEvent in this._Activity.EnumerateEvents()) {
        }
        */
        var value = this.GetValue();

        foreach (ref readonly var tag in value.EnumerateTagObjects()) {
            if (propertyName == tag.Key) {
                propertyValue = tag.Value;
                return true;
            }
        }

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
        var value = this.GetValue();

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
    public TracorIdentitfier? TracorIdentitfier { get; set; }


    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        var value = this.GetValue();
        var enumeratorTagObjects = value.EnumerateTagObjects();

        if (enumeratorTagObjects.MoveNext()) {
            ref readonly var tag = ref enumeratorTagObjects.Current;
            if (tag.Value is { } tagValue) {
                listProperty.Add(TracorDataProperty.Create(tag.Key, tagValue));
            }
        }
    }

    protected override void ResetState() {
        this._Value = null;
        this.TracorIdentitfier = default;
    }

    protected override bool IsStateReseted()
        => this._Value is null;
}