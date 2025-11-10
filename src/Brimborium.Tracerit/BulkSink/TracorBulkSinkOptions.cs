namespace Brimborium.Tracerit.BulkSink;

/// <summary>
/// Use GetApplicationStopping or FlushAsync to prevent loose log entries.
/// </summary>
public class TracorBulkSinkOptions {
    /// <summary>
    /// The period after the buffer will be flushed.
    /// </summary>
    public TimeSpan FlushPeriod { get; set; } = TimeSpan.FromSeconds(1);

    private Func<IServiceProvider, CancellationToken>? _OnGetApplicationStopping;

    /// <summary>
    /// Important allows to retrieve the IHostApplicationLifetime.ApplicationStopping which is essential for periodical flush.
    /// So that at the end the buffer will be flushed.
    /// </summary>
    /// <example>
    /// fileTracorOptions.GetApplicationStopping = static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping
    /// </example>
    public Func<IServiceProvider, CancellationToken>? GetOnGetApplicationStopping() {
        return this._OnGetApplicationStopping;
    }

    public TracorBulkSinkOptions SetOnGetApplicationStopping(Func<IServiceProvider, CancellationToken>? value) {
        this._OnGetApplicationStopping = value;
        return this;
    }

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