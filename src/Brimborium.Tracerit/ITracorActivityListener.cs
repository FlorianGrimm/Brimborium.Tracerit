using Brimborium.Tracerit.Diagnostics;

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
    void AddActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier);

    /// <summary>
    /// Removes an activity source identifier from the instrumentation list.
    /// </summary>
    /// <param name="activitySourceIdentifier">The activity source identifier to remove from monitoring.</param>
    void RemoveActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier);

#if false
    /// <summary>
    /// Adds an activity source type to the list of monitored sources.
    /// </summary>
    /// <typeparam name="T">a type derived from <see cref="ActivitySourceBase"/></typeparam>
    void AddActivitySourceByType<T>() where T : ActivitySourceBase;

    /// <summary>
    /// Removes an activity source type from the list of monitored sources.
    /// </summary>
    /// <typeparam name="T">a type derived from <see cref="ActivitySourceBase"/></typeparam>
    void RemoveActivitySourceByType<T>() where T : ActivitySourceBase;
#endif
}

/// <summary>
/// Configuration options for the TracorActivityListener, controlling which activity sources are monitored.
/// </summary>
public class TracorActivityListenerOptions {
    /// <summary>
    /// Gets or sets a value indicating whether the start event for the activity source is enabled.
    /// </summary>
    public bool ActivitySourceStartEventEnabled { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether the "Stop" event is emitted for activities created by the <see
    /// cref="System.Diagnostics.ActivitySource"/>.
    /// </summary>
    public bool ActivitySourceStopEventEnabled { get; set; } = true;

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

    private readonly List<IActivitySourceResolver> _ListActivitySourceResolver = new();
    public List<IActivitySourceResolver> GetListActivitySourceResolver() => this._ListActivitySourceResolver;
}
