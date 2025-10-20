namespace Brimborium.Tracerit.Service;

/// <summary>
/// Internal
/// </summary>
public sealed class TracorEmergencyLogging {
    private bool _IsEnabled;

    public TracorEmergencyLogging() {
        this._IsEnabled = false;
    }

    public TracorEmergencyLogging(
        IOptions<TracorOptions> options) {
        this._IsEnabled = options.Value.IsEmergencyLogging;
    }

    public bool IsEnabled { get => this._IsEnabled; set => this._IsEnabled = value; }

    public void Log(string message) {
        if (this._IsEnabled) {
            var utcNow = System.DateTime.UtcNow;
            System.Console.Out.WriteLine($"{utcNow:O} {message}");
        }
    }
}
