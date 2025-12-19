using Microsoft.AspNetCore.Authentication.Negotiate;

[assembly: InternalsVisibleTo("Brimborium.Tracerit.Collector.Test")]
[assembly: InternalsVisibleTo("Brimborium.Tracerit.Collector.OOP.Test")]
[assembly: InternalsVisibleTo("Brimborium.Tracerit.Collector.ForTesting")]

namespace Brimborium.Tracerit.Collector;

public sealed class Program {
    public static async Task<int> Main(string[] args) {
        try {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            await RunAsync(builder, new()).ConfigureAwait(false);
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
        WebApplicationBuilder builder,
        StartupActions startupActions
        ) {
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

        builder.Services.AddAngularFileService()
            .Configure(options => {
                options.AngularPathPrefix.Add("tracorit");
            });

        // Add services to the container.
        AppConfig appConfig = new();
        builder.Configuration.Bind(appConfig);

        //if (appConfig.LimitLocalhost) { 
        //    builder.Services.AddAuthentication(LocalHostDefaults.AuthenticationScheme)
        //      .AddLocalHostAuthentication();
        //} else { 
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();
        //}

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddAuthorization(options => {
            // By default, all incoming requests will be authorized according to the default policy.
            options.FallbackPolicy = options.DefaultPolicy;
        });

        builder.Services.AddTracor(
            addEnabledServices: true,
            configuration: builder.Configuration,
            configureTracor: (tracorOptions) => {
                tracorOptions.SetOnGetApplicationStopping(
                    static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
            },
            configureConvert: default,
            tracorScopedFilterSection: default)
            .AddFileTracorCollectiveSinkDefault(
                configuration: builder.Configuration,
                configure: (options) => {
                });

        builder
            .AddTracorCollectorServer(
                configSectionPath: default,
                configure: default)
            .AddTracorCollectorToTracor();

        /*
        builder.Services.AddResponseCompression(options => {
            options.EnableForHttps = true;
            options.MimeTypes =
               ResponseCompressionDefaults.MimeTypes.Concat(
                   new[] { "application/jsonl" });
        });
        */

        builder.Services.AddOptions<AppConfig>().BindConfiguration("");

        if (appConfig.ServiceName is { Length: > 0 } serviceName) {
            builder.Services.AddWindowsService(options => {
                options.ServiceName = serviceName;
            });
        }

        if (startupActions.ConfigureWebApplicationBuilder is { } configureWebApplicationBuilder) { configureWebApplicationBuilder(builder); }

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        if (app.Environment.IsDevelopment()) {
            app.MapOpenApi();
        }

        //if (startupActions.Runtime) {
        //    app.UseHttpsRedirection();
        //}

        //app.UseAuthorization();
        //app.UseAuthentication();
        app.UseAngularFileService();
        app.MapTracorControllerEndpoints();

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