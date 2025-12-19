using Brimborium.Tracerit.Logger;

namespace Brimborium.Tracerit.Collector.Test.TestUtility;

public class WebApplicationFactoryIntegration : IAsyncInitializer {
    private const string _ApplicationName = "Brimborium.Tracerit.Collector.Test";

    private WebApplication? _Application;
    public WebApplication GetApplication() => this._Application ?? throw new InvalidOperationException("Application yet is not set");
    public IServiceProvider GetServices() => this.GetApplication().Services;

    private ITracorServiceSink? _Tracor;
    public ITracorServiceSink GetTracor() => this._Tracor
        ??= this.GetServices().GetRequiredService<ITracorServiceSink>();

    private ITracorValidator? _TracorValidator;
    public ITracorValidator GetTracorValidator() => this._TracorValidator
        ??= this.GetServices().GetRequiredService<ITracorValidator>();


    private string? _AddressHttpQ = null;
    private string? _AddressHttpsQ = null;
    public string GetBaseAddress() {
        if (this._AddressHttpsQ is null || this._AddressHttpQ is null) {
            var server = this.GetApplication().Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
            var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
            if (serverAddressesFeature is { Addresses: { } addresses }) {
                foreach (var addressQ in addresses) {
                    if (addressQ is { Length: > 0 } address) {
                        if (address.StartsWith("http:")) {
                            this._AddressHttpQ = address;
                        }
                        if (address.StartsWith("https:")) {
                            this._AddressHttpsQ = address;
                            break;
                        }
                    }
                }
            }
        }

        return this._AddressHttpsQ ?? this._AddressHttpQ ?? throw new Exception("Cannot find BaseAddress");
    }

    private Flurl.Url? _BaseUrl;
    public Flurl.Url GetBaseUrl() {
        if (this._BaseUrl is Flurl.Url baseUrl) {
            return baseUrl.Clone();
        } else {
            baseUrl = new(this.GetBaseAddress());
            if ("https" == baseUrl.Scheme) {
                if (443 == baseUrl.Port) {
                    baseUrl.Port = null;
                }
            } else if ("http" == baseUrl.Scheme) {
                if (80 == baseUrl.Port) {
                    baseUrl.Port = null;
                }
            }
            if (string.IsNullOrEmpty(baseUrl.Path)) {
                baseUrl.Path = "/";
            }
            this._BaseUrl = baseUrl;
            return baseUrl.Clone();
        }
    }

    public HttpClient CreateClient() {
        // var server = this.GetApplication().Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
        var socketsHandler = new SocketsHttpHandler {
            Credentials = CredentialCache.DefaultCredentials
        };
        var result = new HttpClient(socketsHandler, true);
        if (this.GetBaseAddress() is { Length: > 0 } baseAddress) {
            result.BaseAddress = new Uri(baseAddress);
        }
        return result;
    }

    public async Task InitializeAsync() {
        string pathStaticAssets = GetPathStaticAssets();
        var contentRoot = Program.GetContentRoot();
        var tsc = new TaskCompletionSource<WebApplication>();

        var args = new string[] {
                $"--StaticAssets=\"{pathStaticAssets}\""
            };
        WebApplicationOptions webApplicationOptions = new() { 
            ApplicationName = _ApplicationName,
            EnvironmentName = "Development",
            ContentRootPath = contentRoot,
            Args=args
        };
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        var taskServer = Program.RunAsync(
            builder,
            new StartupActions() {
                Testtime = true,
                ConfigureWebApplicationBuilder = (builder) => {
                    builder.Configuration.AddJsonFile(GetAppsettingsJson(), optional: false);
                    builder.Services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                        .AddScheme<TestAuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, null);
                    builder.Services
                        .AddTracor(
                            addEnabledServices: true,
                            configuration: default,
                            configureTracor: default,
                            configureConvert: default,
                            tracorScopedFilterSection: default)
                        .AddTracorActivityListener(true, null, (options) => {
                            options.AllowAllActivitySource = true;
                        }).AddTracorLogger((options) => { 
                            options.LogLevel = LogLevel.Trace; 
                        });
                    builder.Services.AddReplacements();
                },
                ConfigureWebApplication = (app) => {
                    app.Services.TracorActivityListenerStart();
                    app.Map("/_test/ping", () => {
                        var utcNow= DateTime.UtcNow;
                        return $"pong {utcNow:O}";
                    }).AllowAnonymous();
                },
                RunningWebApplication = (app, task) => {
                    tsc.SetResult(app);
                }
            });
        await Task.Delay(100);
        this._Application = await tsc.Task;
    }

    private static string GetPathStaticAssets() {
        var assemblyLocation = Path.GetDirectoryName(
                typeof(WebApplicationFactoryIntegration).Assembly.Location ?? throw new Exception("")
            ) ?? throw new Exception("");
        var result = Path.Combine(
            assemblyLocation,
            $"{_ApplicationName}.staticwebassets.endpoints.json");
        return result;
    }
    private static string GetAppsettingsJson([CallerFilePath] string callerFilePath = "") {
        var testUtilityDir = System.IO.Path.GetDirectoryName(callerFilePath) ?? throw new Exception();
        var projectDir = System.IO.Path.GetDirectoryName(testUtilityDir) ?? throw new Exception();
        var result = System.IO.Path.Combine(projectDir, "appsettings.json");
        return result;
    }
}