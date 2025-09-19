#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Brimborium.Tracerit.DataAccessor;

public sealed class ActivityTracorDataFactory
    : ITracorDataAccessorFactory<Activity> {
    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is Activity activity) {
            tracorData = new ActivityTracorData(activity);
            return true;
        }
        tracorData = null;
        return false;
    }

    public bool TryGetDataTyped(Activity value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        tracorData = new ActivityTracorData(value);
        return true;
    }
}
