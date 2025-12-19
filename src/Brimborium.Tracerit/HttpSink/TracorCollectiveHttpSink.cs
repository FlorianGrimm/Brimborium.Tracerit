using Microsoft.IO;

namespace Brimborium.Tracerit.HttpSink;

public sealed class TracorCollectiveHttpSink
    : TracorCollectiveBulkSink<TracorHttpSinkOptions> {

    private readonly List<HttpTarget> _ListTargetUrl = [];


    internal TracorCollectiveHttpSink(
        TracorOptions tracorOptions,
        TracorHttpSinkOptions httpSinkOptions,
        TracorMemoryPoolManager tracorRecyclableMemoryStreamManager
        ) : this(
            tracorOptions, httpSinkOptions, tracorRecyclableMemoryStreamManager,
            new()) {
    }

    internal TracorCollectiveHttpSink(
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

        {
            if (options.TestingTargetUrl is { Length: > 8 } targetUrl) {
                addTargetUrl(targetUrl);
            }
        }
        {
            if (options.TargetUrl is { Length: > 8 } targetUrl) {
                addTargetUrl(targetUrl);
            }
        }
        {
            foreach (var targetUrl in options.ListTargetUrl) {
                if (targetUrl is { Length: > 8 }) {
                    addTargetUrl(targetUrl);
                }
            }
        }

        void addTargetUrl(string targetUrl) {
            var uri = new Uri(targetUrl, UriKind.Absolute);
            var resource = Uri.EscapeDataString(this._ApplicationName ?? "Application");
            var uriTargetUrl = new Uri(uri, $"/_api/tracerit/v1/collector.http/{resource}");
            var absoluteUri = uriTargetUrl.AbsoluteUri;

            foreach (var target in this._ListTargetUrl) {
                if (string.Equals(absoluteUri, target.TargetUrl, StringComparison.OrdinalIgnoreCase)) {
                    return;
                }
            }

            this._ListTargetUrl.Add(new(
                absoluteUri,
                this._TracorEmergencyLogging));
        }
    }

    public override bool IsEnabled()
        => this._ListTargetUrl.Count > 0;

    private readonly TracorMemoryPoolManager _TracorMemoryPoolManager;

    protected override async Task WriteAsync(List<ITracorData> listTracorData) {
        if (this._ListTargetUrl.Count == 0) { return; }
        //if (this._TargetUrl is not { Length: > 0 }) { return; }

        List<HttpTarget> listActive = new(this._ListTargetUrl.Count);
        DateTime utcNow = System.DateTime.UtcNow;
        foreach (var target in this._ListTargetUrl) {
            if (target.IsBlocked(utcNow)) {
                // skip
            } else {
                listActive.Add(target);
            }
        }

        if (listActive.Count == 0) { return; }

        // delegate disposing of the stream to WriteAsync.
        var stream = this._TracorMemoryPoolManager.RecyclableMemoryStreamManager.GetStream();
        using (var brotliStream = new BrotliStream(stream, CompressionMode.Compress, true)) {
            await this.ConvertAndWriteAsync(listTracorData, false, this._Resource, brotliStream);
            brotliStream.Flush();
        }
        if (1 == listActive.Count) {
            var target = listActive[0];
            // delegate disposing of the stream.
            await target.WriteAsync(utcNow, stream);
        } else {
            List<Task> tasks = new List<Task>(listActive.Count);
            for (int index = listActive.Count - 1; 0 <= index; index--) {
                HttpTarget? target = listActive[index];
                if (index == 0) {
                    // the last one does not need to copy the stream
                    var task = target.WriteAsync(utcNow, stream);
                    tasks.Add(task);
                } else {
                    // the others must copy the content since WriteAsync dispose the stream.
                    var streamCopy = this._TracorMemoryPoolManager.RecyclableMemoryStreamManager.GetStream();
                    stream.Position = 0;
                    stream.CopyTo(streamCopy);
                    streamCopy.Position = 0;
                    var task = target.WriteAsync(utcNow, streamCopy);
                    tasks.Add(task);
                }
            }
            foreach (var task in tasks) {
                try {
                    await task.ConfigureAwait(false);
                } catch (Exception error) {
                    this._TracorEmergencyLogging.Log($"TracorCollectiveHttpSink: {error.Message}");
                }
            }
        }
    }

    private sealed class HttpTarget {
        private readonly string _TargetUrl;
        private HttpClient? _HttpClient;
        private DateTime _BlockUntil = DateTime.MinValue;
        private long _BlockDelay = 0;
        private TracorEmergencyLogging _TracorEmergencyLogging;


        public HttpTarget(string targetUrl, TracorEmergencyLogging tracorEmergencyLogging) {
            this._TargetUrl = targetUrl;
            this._TracorEmergencyLogging = tracorEmergencyLogging;
        }

        public string TargetUrl => this._TargetUrl;

        public bool IsBlocked(DateTime utcNow) {
            return (utcNow < this._BlockUntil);
        }

        internal async Task WriteAsync(
            DateTime utcNow,
            RecyclableMemoryStream stream) {

            if (this._HttpClient is { } httpClient) {
            } else {
                var handler = new HttpClientHandler();
                /* TODO make this: configureable */
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) => {
                            return true;
                        };
                }
                this._HttpClient = httpClient = new HttpClient(handler);
            }
            try {
                using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(
                    HttpMethod.Post,
                    this._TargetUrl)) {
                    stream.Position = 0;
                    httpRequestMessage.Content = new StreamContent(stream);
                    httpRequestMessage.Content.Headers.ContentType
                        = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json", "utf-8");
                    httpRequestMessage.Content.Headers.ContentEncoding.Add("brotli");

                    using (var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false)) {
                        response.EnsureSuccessStatusCode();
                    }
                }
                this._BlockDelay = 0;
            } catch (System.Exception error) {
                using (var httpClientToDispose = this._HttpClient) {
                    this._HttpClient = null;
                }
                this._BlockDelay = System.Math.Min(
                        60 * 15,
                        System.Math.Max(1,
                            2 + (this._BlockDelay * 3) / 2));
                this._BlockUntil = utcNow.AddSeconds(this._BlockDelay + 30);
                if (this._TracorEmergencyLogging.IsEnabled) {
                    this._TracorEmergencyLogging.Log($"TracorCollectiveHttpSink error while transport {error.Message}.");
                }
            }
        }
    }

}
