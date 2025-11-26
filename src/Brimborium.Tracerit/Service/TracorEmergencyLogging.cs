namespace Brimborium.Tracerit.Service;

/// <summary>
/// Emergency logging service for debugging Tracor itself by writing to <see cref="System.Console"/>.
/// Used for internal diagnostics when the main tracing system cannot be used.
/// </summary>
public sealed class TracorEmergencyLogging {
    private bool _IsEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorEmergencyLogging"/> class with logging disabled.
    /// </summary>
    public TracorEmergencyLogging() {
        this._IsEnabled = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorEmergencyLogging"/> class using the provided options.
    /// </summary>
    /// <param name="options">The Tracor options containing the emergency logging configuration.</param>
    public TracorEmergencyLogging(
        IOptions<TracorOptions> options) {
        this._IsEnabled = options.Value.IsEmergencyLogging;
    }

    /// <summary>
    /// Gets or sets whether emergency logging is enabled.
    /// </summary>
    public bool IsEnabled { get => this._IsEnabled; set => this._IsEnabled = value; }

    /// <summary>
    /// Logs a message to the console if emergency logging is enabled.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Log(string message) {
        if (this._IsEnabled) {
            var utcNow = System.DateTime.UtcNow;
            System.Console.Out.WriteLine($"{utcNow:O} {message}");
        }
    }
}
