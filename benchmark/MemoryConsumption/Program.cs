using Brimborium.Tracerit.Expression;

namespace MemoryConsumption;


public class Program {
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

    public static async Task RunAsync(string[] args) {
        HostApplicationBuilder builder = new(args);
        builder.Logging.ClearProviders();
        _ = builder.Logging.AddConfiguration(
            builder.Configuration.GetSection("Logging"));
        var tracorOptions = builder.Configuration.BindTracorOptionsDefault(new());
        var tracorEnabled = tracorOptions.IsEnabled;
        builder.Services.AddTracor(
                addEnabledServices: tracorEnabled,
                configuration: builder.Configuration,
                configureTracor: default, /* (options) => builder.Configuration.BindTracorOptionsDefault(options), */
                configureConvert: default,
                tracorScopedFilterSection: default)
            //.AddFileTracorCollectiveSinkDefault(
            //   configuration: builder.Configuration,
            //   configure: (fileTracorOptions) => {
            //       fileTracorOptions.GetApplicationStopping = static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
            //   })
            .AddTracorActivityListener(
                enabled: tracorEnabled,
                configuration: default,
                configure: default)
            .AddTracorInstrumentation<MemoryConsumptionInstrumentation>()
            .AddTracorValidatorService((options) => {
                options.AddValidator(Create());
            })
            .AddTracorLogger()
            ;
        builder.Services.AddSingleton<ControlService>();
        builder.Services.AddHostedService<WriterService>();

        builder.Services.Add(ServiceDescriptor.Singleton<BackgroundValidatorService, BackgroundValidatorService>());
        builder.Services.Add(ServiceDescriptor.Singleton<IHostedService>((sp) => sp.GetRequiredService<BackgroundValidatorService>()));

        var host = builder.Build();
        //host.Services.GetRequiredService<BackgroundValidatorService>().Init();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Start App {MachineName}", System.Environment.MachineName);
        var fileTracorCollectiveSink = host.Services.GetRequiredService<TracorCollectiveFileSink>();
        await fileTracorCollectiveSink.FlushAsync();

        await host.RunAsync();

        logger.LogInformation("Stop App {MachineName}", System.Environment.MachineName);
    }

    public static ValidatorExpression Create() {
        var result = new GroupByRootActivityExpression("");
        return result;
    }
}

public class ControlService {
    private readonly IHostApplicationLifetime _HostApplicationLifetime;

    public ControlService(
        IHostApplicationLifetime hostApplicationLifetime) {
        this._HostApplicationLifetime = hostApplicationLifetime;
    }


    internal Task WriterDone() {
        this._HostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}

public class WriterService : BackgroundService {
    private readonly ControlService _ControlService;
    private readonly TracorCollectiveFileSink _FileTracorCollectiveSink;
    private readonly ILogger<WriterService> _Logger;

    public WriterService(
        ControlService controlService,
        TracorCollectiveFileSink fileTracorCollectiveSink,
        ILogger<WriterService> logger
        ) {
        this._ControlService = controlService;
        this._FileTracorCollectiveSink = fileTracorCollectiveSink;
        this._Logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            for (int outerloop = 0; outerloop < 10; outerloop++) {
                int sum = 0;
                for (int innerloop = 0; innerloop < 1000; innerloop++) {
                    this._Logger.WriterInnerLoop(innerloop);
                    sum += innerloop;
                }
                this._Logger.WriterInnerSum(sum);
                await this._FileTracorCollectiveSink.FlushAsync();
                await Task.Delay(TimeSpan.FromMilliseconds(100), CancellationToken.None);
            }
        } finally {
            await this._ControlService.WriterDone();
        }
    }
}

public static partial class ReadAndWriteLoggerExtension {
    [LoggerMessage(LogLevel.Information, "Inner loop iteration:{number}")]
    public static partial void WriterInnerLoop(this ILogger logger, int number);

    [LoggerMessage(LogLevel.Information, "Inner loop sum:{sum}")]
    public static partial void WriterInnerSum(this ILogger logger, int sum);
}

public class BackgroundValidatorService : BackgroundService {
    private readonly TracorValidatorService _TracorValidatorService;

    public BackgroundValidatorService(
        TracorValidatorService tracorValidatorService) {
        this._TracorValidatorService = tracorValidatorService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await this._TracorValidatorService.ExecuteAsync(stoppingToken);
    }
}