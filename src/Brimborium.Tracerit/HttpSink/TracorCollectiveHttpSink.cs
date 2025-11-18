using Microsoft.IO;

namespace Brimborium.Tracerit.HttpSink;

public sealed class TracorCollectiveHttpSink
    : TracorCollectiveBulkSink<TracorHttpSinkOptions> {

    private string? _TargetUrl;

    public TracorCollectiveHttpSink(
        TracorOptions tracorOptions,
        TracorHttpSinkOptions httpSinkOptions,
        TracorMemoryPoolManager tracorRecyclableMemoryStreamManager
        ) : this(
            tracorOptions, httpSinkOptions, tracorRecyclableMemoryStreamManager,
            new()) {
    }

    public TracorCollectiveHttpSink(
        TracorOptions tracorOptions,
        TracorHttpSinkOptions httpSinkOptions,
        TracorMemoryPoolManager tracorRecyclableMemoryStreamManager,
        TracorEmergencyLogging tracorEmergencyLogging)
        : base(tracorOptions, httpSinkOptions, tracorEmergencyLogging) {
        this._TracorMemoryPoolManager = tracorRecyclableMemoryStreamManager;
    }

    public TracorCollectiveHttpSink(
        IServiceProvider serviceProvider,
        IOptionsMonitor<TracorOptions> tracorOptions,
        IOptionsMonitor<TracorHttpSinkOptions> httpSinkOptions,
        TracorMemoryPoolManager tracorRecyclableMemoryStreamManager,
        TracorEmergencyLogging tracorEmergencyLogging
        ) : base(serviceProvider, tracorOptions, httpSinkOptions, tracorEmergencyLogging) {
        this._TracorMemoryPoolManager = tracorRecyclableMemoryStreamManager;
    }

    internal override void SetBulkSinkOptionsExtended(TracorHttpSinkOptions options) {
        base.SetBulkSinkOptionsExtended(options);
        this._TargetUrl = options.TargetUrl;
    }

    public override bool IsEnabled()
        => this._TargetUrl is { Length: > 0 };

    private HttpClient? _HttpClient;
    private readonly TracorMemoryPoolManager _TracorMemoryPoolManager;

    protected override async Task WriteAsync(List<ITracorData> listTracorData) {
        if (this._TargetUrl is not { Length: > 0 }) { return; }
        if (this._HttpClient is { } httpClient) {
        } else {
            this._HttpClient = httpClient = new HttpClient();
        }
        try {
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                this._TargetUrl)) {
                using (var stream = this._TracorMemoryPoolManager.RecyclableMemoryStreamManager.GetStream()) {
                    using (var brotliStream = new BrotliStream(stream, CompressionMode.Compress, true)) {
                        await this.ConvertAndWriteAsync(listTracorData, false, true, brotliStream);
                        brotliStream.Flush();
                    }

                    stream.Position = 0;
                    httpRequestMessage.Content = new StreamContent(stream);
                    httpRequestMessage.Content.Headers.ContentType
                        = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json", "utf-8");
                    httpRequestMessage.Content.Headers.ContentEncoding.Add("brotli");

                    using (var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false)) {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        } catch {
            this._HttpClient?.Dispose();
            this._HttpClient = null;
        }
    }
}
