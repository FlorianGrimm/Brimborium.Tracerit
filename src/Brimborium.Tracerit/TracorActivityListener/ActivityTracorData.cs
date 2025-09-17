namespace Brimborium.Tracerit.DataAccessor;

public sealed class ActivityTracorData
    : ITracorData<Activity> {
    private readonly Activity _Activity;

    public ActivityTracorData(Activity activity) {
        this._Activity = activity;
    }

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
}
