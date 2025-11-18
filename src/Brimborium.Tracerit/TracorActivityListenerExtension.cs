#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;

public static class TracorActivityListenerExtension {
    public static bool TracorActivityListenerStart(
        this IServiceProvider serviceProvider) {
        var tracorActivityListener = serviceProvider.GetService<ITracorActivityListener>();
        if (tracorActivityListener is { }) {
            tracorActivityListener.Start();
            return true;
        }
        return false;
    }
    public static void TracorActivityListenerStop(
        this IServiceProvider serviceProvider) {
        var tracorActivityListener = serviceProvider.GetService<ITracorActivityListener>();
        if (tracorActivityListener is { }) {
            tracorActivityListener.Stop();
        }
    }
}