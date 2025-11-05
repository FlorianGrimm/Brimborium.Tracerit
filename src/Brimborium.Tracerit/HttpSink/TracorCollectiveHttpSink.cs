using Microsoft.IO;

namespace Brimborium.Tracerit.HttpSink;

public sealed class TracorCollectiveHttpSink
    : TracorCollectiveBulkSink<TracorHttpSinkOptions> {
    private static readonly RecyclableMemoryStreamManager _Manager = new RecyclableMemoryStreamManager();

    private string? _TargetUrl;

    public TracorCollectiveHttpSink(
        TracorOptions tracorOptions,
        TracorHttpSinkOptions httpSinkOptions
        ) : this(tracorOptions, httpSinkOptions, new()) {
    }

    public TracorCollectiveHttpSink(
        TracorOptions tracorOptions,
        TracorHttpSinkOptions httpSinkOptions,
        TracorEmergencyLogging tracorEmergencyLogging)
        : base(tracorOptions, httpSinkOptions, tracorEmergencyLogging) {
    }

    public TracorCollectiveHttpSink(
        IServiceProvider serviceProvider,
        IOptionsMonitor<TracorOptions> tracorOptions,
        IOptionsMonitor<TracorHttpSinkOptions> httpSinkOptions,
        TracorEmergencyLogging tracorEmergencyLogging
        ) : base(serviceProvider, tracorOptions, httpSinkOptions, tracorEmergencyLogging) {
    }

    internal override void SetBulkSinkOptionsExtended(TracorHttpSinkOptions options) {
        base.SetBulkSinkOptionsExtended(options);
        this._TargetUrl = options.TargetUrl;
    }

    public override bool IsEnabled()
        => this._TargetUrl is { Length: > 0 };

    private HttpClient? _HttpClient;

    protected override async Task WriteAsync(List<ITracorData> listTracorData) {
        if (this._HttpClient is { } httpClient) {
        } else {
            this._HttpClient = httpClient = new HttpClient();
        }
        try {
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                this._TargetUrl)) {
                using (var stream = _Manager.GetStream()) {
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
