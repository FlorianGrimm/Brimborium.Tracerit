[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Sample.Test")]

namespace Sample.WebApp;

public partial class Program {
    public static async Task<int> Main(string[] args) {
        try {
            await RunAsync(args).ConfigureAwait(false);
            return 0;
        } catch (Microsoft.Extensions.Hosting.HostAbortedException) {
            return 0;
        } catch (AggregateException error) {
            System.Console.Error.WriteLine(error.ToString());
            error.Handle((e) => {
                System.Console.Error.WriteLine(e.ToString());
                return true;
            });
            return 1;
        } catch (Exception error) {
            System.Console.Error.WriteLine(error.ToString());
            return 1;
        }
    }

    internal static Task RunAsync(
        string[] args,
        StartupActions? startupActions = default
        ) {
        startupActions ??= new();
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddAngularFileService()
            .Configure(options => {
                options.AngularPathPrefix.Add("home");
                options.AngularPathPrefix.Add("page1");
                options.AngularPathPrefix.Add("page2");
            });
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();

        /*
        builder.Services.AddHealthChecks().AddTypeActivatedCheck<>();
        */

#if false
        {
            var serviceName = SampleInstrumentation.ActivitySourceName;
            var serviceVersion = SampleInstrumentation.ActivitySourceVersion;
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(
                    resource => resource
                        .AddService(
                            serviceName: serviceName,
                            serviceVersion: serviceVersion))
                        .WithTracing(tracing => tracing
                            .AddSource(serviceName)
                            //.AddAspNetCoreInstrumentation()
                            .AddConsoleExporter())
                        .WithMetrics(metrics => metrics
                            .AddMeter(serviceName)
                            .AddConsoleExporter());

            builder.Logging.AddOpenTelemetry(
                options => options
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(
                                serviceName: serviceName,
                                serviceVersion: serviceVersion))
                    .AddConsoleExporter());

        }
#endif

        builder.Services.AddSingleton(new SampleInstrumentation());

        if (startupActions.ConfigureWebApplicationBuilder is { } configureWebApplicationBuilder) { configureWebApplicationBuilder(builder); }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        if (startupActions.Runtime) {
            app.UseHttpsRedirection();
        }

        /*
        app.MapHealthChecks("/healthz").AllowAnonymous();
        */

        app.UseAuthentication();

        app.UseRouting();

        app.UseAuthorization();

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        app.MapGet("/ping", (HttpContext httpContext) => {
            var now = System.DateTimeOffset.Now;
            var result = $"pong {now:u}";
            logger.LogTrace("pong {now}", now);
            return result;
        }).AllowAnonymous();

        app.UseAngularFileService();

        if (startupActions.ConfigureWebApplication is { } configureWebApplication) { configureWebApplication(app); }
        var taskRun = app.RunAsync();

        if (startupActions.RunningWebApplication is { } runningWebApplication) { runningWebApplication(app, taskRun); }

        return taskRun;
    }

    // for test
    internal static string GetContentRoot() {
        return _GetContentRoot();

        static string _GetContentRoot([System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "") {
            if (System.IO.Path.GetDirectoryName(callerFilePath) is { Length: > 0 } result) {
                return result;
            } else {
                throw new InvalidOperationException("GetContentRoot failed");
            }
        }
    }
}

internal class StartupActions {
    public bool Runtime { get; init; } = true;
    public bool Testtime { get => !this.Runtime; init { this.Runtime = !value; } }
    public Action<WebApplicationBuilder> ConfigureWebApplicationBuilder { get; init; } = (_) => { };
    public Action<WebApplication> ConfigureWebApplication { get; init; } = (_) => { };
    public Action<WebApplication, Task> RunningWebApplication { get; init; } = (_, _) => { };
}