using Brimborium.Tracerit;
using Brimborium.Tracerit.Filter;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sample.WebApp;
using System.Runtime.CompilerServices;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Sample.Test")]

namespace Sample.WebApp;

public partial class Program {
    public static async Task<int> Main(string[] args) {
        try {
            await RunAsync(args, new()).ConfigureAwait(false);
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
        StartupActions startupActions
        ) {
        var builder = WebApplication.CreateBuilder(args);
#if DEBUG
        Brimborium.Tracerit.Utility.TracorTestingUtility.WireParentTestingProcessForTesting(builder.Configuration);
#endif

        _ = builder.Logging.AddConfiguration(
            builder.Configuration.GetSection("Logging"));

        // Add services to the container.
        //builder.Services.AddRazorPages((RazorPagesOptions rpo) => { });

        builder.Services.AddAngularFileService()
            .Configure(options => {
                options.AngularPathPrefix.Add("home");
                options.AngularPathPrefix.Add("page1");
                options.AngularPathPrefix.Add("page2");
            });
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();
        
        builder.Services.AddAuthorization();
        /*
        builder.Services.AddHealthChecks().AddTypeActivatedCheck<>();
        */

        {
#if true
            var serviceName = SampleInstrumentation.ActivitySourceName;
            var serviceVersion = SampleInstrumentation.ActivitySourceVersion;
            builder.Logging.AddOpenTelemetry();

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(
                    resource => resource
                        .AddService(
                            serviceName: serviceName,
                            serviceVersion: serviceVersion))
                        .WithTracing(tracing => tracing
                            .AddSource(serviceName)
                            //.AddAspNetCoreInstrumentation()
                            //.AddConsoleExporter()
                            .AddAspNetCoreInstrumentation()
                            .AddOtlpExporter((otlpExporterOptions) => {
                                otlpExporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                                otlpExporterOptions.Endpoint = new Uri("http://localhost:4318/v1/traces");
                                //otlpExporterOptions.Endpoint = new Uri("http://localhost:8080/v1/traces");
                            })
                            )
                        .WithLogging(
                            (loggerProviderBuilder) => {
                                loggerProviderBuilder.AddOtlpExporter((otlpExporterOptions) => {
                                    otlpExporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                                    otlpExporterOptions.Endpoint = new Uri("http://localhost:4318/v1/logs");
                                    //otlpExporterOptions.Endpoint = new Uri("http://localhost:8080/v1/logs");
                                    //4318/v1/traces
                                });
                            }, (openTelemetryLoggerOptions) => {
                                openTelemetryLoggerOptions.IncludeFormattedMessage = true;
                            })
                         //.WithMetrics(metrics => metrics
                         //    .AddMeter(serviceName)
                         //    .AddConsoleExporter())

                         ;

            //builder.Logging.AddOpenTelemetry(
            //    options => options
            //        .SetResourceBuilder(
            //            ResourceBuilder.CreateDefault()
            //                .AddService(
            //                    serviceName: serviceName,
            //                    serviceVersion: serviceVersion))
            //        );

#endif
        }
        var tracorOptions = builder.Configuration.BindTracorOptionsDefault(new());
        var tracorEnabled = tracorOptions.IsEnabled || startupActions.Testtime;
        builder.Services.AddTracor(
            addEnabledServices: tracorEnabled,
            configureTracor: (tracorOptions) => {
                tracorOptions.SetOnGetApplicationStopping(static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
            },
            configureConvert: null,
            tracorScopedFilterSection: ""
            )
            .AddTracorActivityListener(
                enabled: tracorEnabled,
                configuration: default,
                configure: (options) => { 
                    //options.AllowAllActivitySource = true;
                })
            .AddTracorInstrumentation<SampleInstrumentation>()
            .AddTracorLogger()
            .AddFileTracorCollectiveSinkDefault(
               configuration: builder.Configuration,
               configure: (fileTracorOptions) => {
               })
            .AddTracorCollectiveHttpSink(
               configuration: builder.Configuration,
               configure: (tracorHttpSinkOptions) => {
                    //tracorHttpSinkOptions.TargetUrl = "http://localhost:8080/_api/tracerit/v1/collector.http";
                });
        ;

        if (startupActions.ConfigureWebApplicationBuilder is { } configureWebApplicationBuilder) { configureWebApplicationBuilder(builder); }

        var app = builder.Build();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Start App {MachineName}", System.Environment.MachineName);
        app.Services.TracorActivityListenerStart();
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        if (startupActions.Runtime) {
            app.UseHttpsRedirection();
        }

        // app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping

        /*
            app.MapHealthChecks("/healthz").AllowAnonymous();
        */

        app.UseAuthentication();

        app.UseRouting();

        app.UseAuthorization();

        app.MapGet("/ping", async (HttpContext httpContext) => {
            var aspActivity = Activity.Current;
            using (var activity = httpContext.RequestServices.GetRequiredService<SampleInstrumentation>().StartRoot()) {
                var now = System.DateTimeOffset.Now;
                var result = $"pong {now:u}";
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                logger.PingResult(now);
                if (activity.Activity is { } sysActivity
                    && aspActivity is { }) {
                    sysActivity.AddLink(new ActivityLink(aspActivity.Context, null));
                    sysActivity.AddTag("Tag", "me");
                }
                return result;
            }
        }).AllowAnonymous();

        app.MapGet("/hack", (HttpContext httpContext) => {
            var tracor = httpContext.RequestServices.GetRequiredService<ITracorSink<Program>>();
            var now = System.DateTimeOffset.Now;
            var result = $"hack {now:u}";
            tracor.GetPrivateTracor(LogLevel.Information, "hack1").TracePrivate(now);
            if (tracor.GetPrivateTracor(LogLevel.Information, "hack2") is { Enabled: true } tracorEnabled) {
                tracorEnabled.TracePrivate(now);
            }
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
    public static string GetCsproj() {
        return System.IO.Path.Combine(GetProjectSourceCodeFolder(), "Sample.csproj");

        static string GetProjectSourceCodeFolder([CallerFilePath] string thisFilePath = "") {
            return System.IO.Path.GetDirectoryName(thisFilePath) ?? throw new Exception("no source code");
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