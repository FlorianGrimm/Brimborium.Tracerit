#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;

public static class TracorActivityListenerExtension {
    public static void TracorActivityListenerStart(
        this IServiceProvider serviceProvider) {
        var tracorActivityListener = serviceProvider.GetService<ITracorActivityListener>();
        if (tracorActivityListener is { }) {
            tracorActivityListener.Start();
        }
    }
    public static void TracorActivityListenerStop(
        this IServiceProvider serviceProvider) {
        var tracorActivityListener = serviceProvider.GetService<ITracorActivityListener>();
        if (tracorActivityListener is { }) {
            tracorActivityListener.Stop();
        }
    }
}