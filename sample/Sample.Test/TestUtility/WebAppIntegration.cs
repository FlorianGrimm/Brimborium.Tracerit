using Brimborium.Tracerit.Logger;

namespace Sample.WebApp.TestUtility;

public class WebAppIntegration : IAsyncInitializer {
    public async Task InitializeAsync() {
        string pathStaticAssets = GetPathStaticAssets();
        var contentRoot = Program.GetContentRoot();
        var tsc = new TaskCompletionSource<WebApplication>();

        var extendedArgs = new string[] {
                    $"--StaticAssets={pathStaticAssets}"
                };
        WebApplicationOptions webApplicationOptions = new () {
            ApplicationName = "Sample",
            EnvironmentName = "Development",
            ContentRootPath = contentRoot,
            Args = extendedArgs
        };
        var builder = WebApplication.CreateBuilder(webApplicationOptions);
        Brimborium.Tracerit.Utility.TracorTestingUtility.WireParentTestingProcessForTesting(builder.Configuration);

        var taskServer = Program.RunAsync(
            builder: builder,
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
                        })
                        .AddTracorLogger((options) => {
                            options.LogLevel = LogLevel.Trace;
                        });
                    builder.Services.AddReplacements();
                },
                ConfigureWebApplication = (app) => {
                    app.Services.TracorActivityListenerStart();
                },
                RunningWebApplication = (app, task) => {
                    tsc.SetResult(app);
                }
            });
        await Task.Delay(100);
        this._Application = await tsc.Task;
    }

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

    private static string GetPathStaticAssets() {
        var assemblyLocation = Path.GetDirectoryName(
                typeof(WebAppIntegration).Assembly.Location ?? throw new Exception("")
            ) ?? throw new Exception("");
        var result = Path.Combine(
            assemblyLocation,
            "Sample.staticwebassets.endpoints.json");
        return result;
    }

    private static string GetAppsettingsJson() {
        var testUtilityDir = GetTestUtilityFolder();
        var projectDir = System.IO.Path.GetDirectoryName(testUtilityDir) ?? throw new Exception();
        var result = System.IO.Path.Combine(projectDir, "appsettings.json");
        return result;

        static string GetTestUtilityFolder([System.Runtime.CompilerServices.CallerFilePath] string thisFilePath = "") {
            return System.IO.Path.GetDirectoryName(thisFilePath) ?? throw new Exception("no source code");
        }
    }
}