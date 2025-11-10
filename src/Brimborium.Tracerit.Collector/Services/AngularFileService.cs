using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Brimborium.Tracerit.Collector.Services;

public sealed class AngularFileService : EndpointDataSource {
    private readonly IWebHostEnvironment _WebHostEnvironment;
    private readonly HashSet<string> _AngularPathPrefix = new HashSet<string>();
    private readonly CancellationTokenSource _CancellationTokenSource;
    private readonly CancellationChangeToken _ChangeToken;

    public AngularFileService(
        IWebHostEnvironment webHostEnvironment,
        IOptions<AngularFileServiceOptions> angularFileServiceOptions
        ) {
        this._CancellationTokenSource = new CancellationTokenSource();
        this._ChangeToken = new CancellationChangeToken(this._CancellationTokenSource.Token);
        this._WebHostEnvironment = webHostEnvironment;
        foreach (var angularPath in angularFileServiceOptions.Value.AngularPathPrefix) {
            var prefix = GetPatternFromAngularPath(angularPath, true);
            this._AngularPathPrefix.Add(prefix);
        }
    }

    public void Initialize() {
        var webRootPath = this._WebHostEnvironment.WebRootPath;
        var webRootProvider = this._WebHostEnvironment.WebRootFileProvider;
        var additionalPathProvider = new PhysicalFileProvider(
            System.IO.Path.Combine(webRootPath, "static/browser")
            );

        var compositeProvider = new CompositeFileProvider(
            webRootProvider,
            additionalPathProvider);

        // Update the default provider.
        this._WebHostEnvironment.WebRootFileProvider = compositeProvider;
    }

    public Task Execute(HttpContext context, Func<Task> next) {
        var requestPath = context.Request.Path;
        if (context.GetEndpoint() is null) {
            if ((requestPath == "") || (requestPath == "/")) {
                return this.WriteIndexHtml(context);
            }
        }
        return next.Invoke();
    }

    private async Task WriteIndexHtml(HttpContext context) {
        var fileInfo = this._WebHostEnvironment.WebRootFileProvider.GetFileInfo("index.html");
        if (fileInfo.Exists) {
            context.Response.ContentType = "text/html";
            context.Response.ContentLength = fileInfo.Length;
            using (var fileStream = fileInfo.CreateReadStream()) {
                await fileStream.CopyToAsync(context.Response.Body, context.RequestAborted);
            }
        } else {
            var output = System.Text.Encoding.UTF8.GetBytes(
                "<html><body>Not Found</body></html>");
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/html";
            context.Response.ContentLength = output.Length;
            await context.Response.Body.WriteAsync(output, context.RequestAborted);
        }
    }

    public override IReadOnlyList<Endpoint> Endpoints {
        get {
            HashSet<string> hsPatterns = new();
            List<Endpoint> result = new List<Endpoint>();
            foreach (var angularPath in this._AngularPathPrefix) {
                {
                    var pattern = GetPatternFromAngularPath(angularPath, false);
                    if (hsPatterns.Add(pattern)) {
                        var ep = CreateEndpoint(pattern, async context => {
                            await WriteIndexHtml(context);
                        });
                        result.Add(ep);
                    }
                }
                {
                    var patternRest = GetPatternFromAngularPath(angularPath, true);
                    if (hsPatterns.Add(patternRest)) {
                        var ep = CreateEndpoint(patternRest, async context => {
                            await WriteIndexHtml(context);
                        });
                        result.Add(ep);
                    }
                }
            }
            return result;
        }
    }

    private static char[] _GetPatternFromAngularPathChars = new char[] { '/', '?', '{' };
    public static string GetPatternFromAngularPath(string angularPath, bool addRest) {
        if (string.IsNullOrEmpty(angularPath) || "/" == angularPath) {
            return "/";
        }
        if (!angularPath.StartsWith("/")) { angularPath = "/" + angularPath; }
        var pos = angularPath.IndexOfAny(_GetPatternFromAngularPathChars, 1);
        if (0 < pos) {
            angularPath = angularPath.Substring(0, pos);
        }
        if (addRest) {
            return angularPath + "/{**rest}";
        } else {
            return angularPath;
        }
    }

    private static Endpoint CreateEndpoint(string pattern, RequestDelegate requestDelegate) =>
        new RouteEndpointBuilder(
            requestDelegate: requestDelegate,
            routePattern: RoutePatternFactory.Parse(pattern),
            order: 0)
        .Build();

    public override IChangeToken GetChangeToken() => this._ChangeToken;
}

public class AngularFileServiceOptions {
    public List<string> AngularPathPrefix { get; set; } = new();
}