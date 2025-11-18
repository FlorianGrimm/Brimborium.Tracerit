namespace Brimborium.Tracerit.BulkSink;

/// <summary>
/// Use GetApplicationStopping or FlushAsync to prevent loose log entries.
/// </summary>
public class TracorBulkSinkOptions {
    /// <summary>
    /// The period after the buffer will be flushed.
    /// </summary>
    public TimeSpan FlushPeriod { get; set; } = TimeSpan.FromSeconds(1);

    private TracorDataRecord? _Resource;

    /// <summary>
    /// The resource is transmitted for each new file, session...
    /// </summary>
    /// <returns>optional - the resource</returns>
    public TracorDataRecord? GetResource() => this._Resource;

    /// <summary>
    /// The resource
    /// </summary>
    /// <param name="value">the resource</param>
    public void SetResource(TracorDataRecord? value) {
        this._Resource = value;
    }

    private JsonSerializerOptions? _JsonSerializerOptions;

    public System.Text.Json.JsonSerializerOptions GetJsonSerializerOptions() {
        if (this._JsonSerializerOptions == null) {
            this._JsonSerializerOptions =
                TracorDataSerialization.GetMinimalJsonSerializerOptions(null, null);
        }
        return this._JsonSerializerOptions;
    }

    public void SetJsonSerializerOptions(System.Text.Json.JsonSerializerOptions? value) {
        this._JsonSerializerOptions = value;
    }
}