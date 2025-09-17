namespace Brimborium.Tracerit;

/// <summary>
/// Defines a listener interface for tracing activities from various activity sources.
/// Provides methods to start/stop listening and manage activity source subscriptions.
/// </summary>
public interface ITracorActivityListener {
    /// <summary>
    /// Starts the activity listener to begin monitoring activity sources.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the activity listener and ceases monitoring activity sources.
    /// </summary>
    void Stop();

    /// <summary>
    /// Adds an activity source name to the list of monitored sources.
    /// </summary>
    /// <param name="name">The name of the activity source to monitor.</param>
    void AddActivitySourceName(string name);

    /// <summary>
    /// Removes an activity source name from the list of monitored sources.
    /// </summary>
    /// <param name="name">The name of the activity source to stop monitoring.</param>
    void RemoveActivitySourceName(string name);

    /// <summary>
    /// Adds an activity source identifier to the instrumentation list for detailed monitoring.
    /// </summary>
    /// <param name="activitySourceIdentifier">The activity source identifier containing name and version information.</param>
    void AddListInstrumentation(ActivitySourceIdentifier activitySourceIdentifier);

    /// <summary>
    /// Removes an activity source identifier from the instrumentation list.
    /// </summary>
    /// <param name="activitySourceIdentifier">The activity source identifier to remove from monitoring.</param>
    void RemoveListInstrumentation(ActivitySourceIdentifier activitySourceIdentifier);
}

/// <summary>
/// Configuration options for the TracorActivityListener, controlling which activity sources are monitored.
/// </summary>
public class TracorActivityListenerOptions {
    /// <summary>
    /// Gets or sets a value indicating whether all activity sources should be monitored automatically.
    /// When true, all activity sources will be listened to regardless of specific name or identifier lists.
    /// </summary>
    public bool AllowAllActivitySource { get; set; }

    /// <summary>
    /// Gets or sets the list of activity source names to monitor.
    /// Only activity sources with names in this list will be monitored when <see cref="AllowAllActivitySource"/> is false.
    /// </summary>
    public List<string> ListActivitySourceName { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of specific activity source identifiers to monitor.
    /// Provides more granular control by specifying both name and version of activity sources to monitor.
    /// </summary>
    public List<ActivitySourceIdentifier> ListActivitySourceIdenifier { get; set; } = new();
}

/// <summary>
/// Represents an identifier for an activity source, containing both name and version information.
/// Used to uniquely identify and filter activity sources for monitoring purposes.
/// </summary>
/// <param name="Name">The name of the activity source.</param>
/// <param name="Version">The version of the activity source. Defaults to an empty string if not specified.</param>
public record struct ActivitySourceIdentifier(
    string Name,
    string Version = ""
    ) {
    /// <summary>
    /// Creates an ActivitySourceIdentifier with the specified name and version.
    /// If the version is null or empty, it defaults to an empty string.
    /// </summary>
    /// <param name="name">The name of the activity source.</param>
    /// <param name="version">The version of the activity source. Can be null or empty.</param>
    /// <returns>A new ActivitySourceIdentifier instance.</returns>
    public static ActivitySourceIdentifier Create(string name, string? version) {
        if (version is null || version is { Length: 0 }) {
            return new ActivitySourceIdentifier(name, string.Empty);
        } else {
            return new ActivitySourceIdentifier(name, version);
        }
    }
}
