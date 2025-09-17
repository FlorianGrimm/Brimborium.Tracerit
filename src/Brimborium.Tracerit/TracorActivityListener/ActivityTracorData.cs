namespace Brimborium.Tracerit.DataAccessor;

public sealed class ActivityTracorData
    : ITracorData<Activity> {
    private readonly Activity _Activity;

    public ActivityTracorData(Activity activity) {
        this._Activity = activity;
    }

    public Activity Activity => this._Activity;

    public List<string> GetListPropertyName() {
        List<string> result = new();
        return result;
    }

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out Activity value) {
        value = this._Activity;
        return true;
    }
    public object? this[string propertyName] => this.TryGetPropertyValue(propertyName, out var value) ? value : null;

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        /*
        foreach (ref readonly var activityEvent in this._Activity.EnumerateEvents()) {
        }
        */
        if (this._Activity.GetCustomProperty(propertyName) is { } customPropertyValue) {
            propertyValue = customPropertyValue;
            return true;
        }

        foreach (ref readonly var tag in this._Activity.EnumerateTagObjects()) {
            if (tag.Key == propertyName) {
                propertyValue= tag.Value;
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
        foreach (ref readonly var tag in this._Activity.EnumerateTagObjects()) {
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
        var enumerator = this._Activity.EnumerateTagObjects();

        if (enumerator.MoveNext()) {
            ref readonly var tag = ref enumerator.Current;

            if (tag.Key == tagName) {
                tagValue = tag.Value;
                return true;
            }
        }

        tagValue = null;
        return false;
    }

    public bool TryGetTagValue<T>(string tagName, [MaybeNullWhen(false)] out T tagValue) {
        var enumerator = this._Activity.EnumerateTagObjects();

        if (enumerator.MoveNext()) {
            ref readonly var tag = ref enumerator.Current;

            if (tag.Key == tagName && tag.Value is T tValue) {
                tagValue = tValue;
                return true;
            }
        }

        tagValue = default;
        return false;
    }

}
