#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// TODO
/// </summary>
public static class TracorActivityListenerExtension {
    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static bool TracorActivityListenerStart(
        this IServiceProvider serviceProvider) {
        var tracorActivityListener = serviceProvider.GetService<ITracorActivityListener>();
        if (tracorActivityListener is { }) {
            tracorActivityListener.Start();
            return true;
        }
        return false;
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static void TracorActivityListenerStop(
        this IServiceProvider serviceProvider) {
        var tracorActivityListener = serviceProvider.GetService<ITracorActivityListener>();
        if (tracorActivityListener is { }) {
            tracorActivityListener.Stop();
        }
    }
}