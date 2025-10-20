namespace ReadAndWrite;

public class Program {
    //public static TimeSpan TimeSpanPeriod = TimeSpan.FromSeconds(2);
    public static TimeSpan TimeSpanPeriod = TimeSpan.FromMilliseconds(1);
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
                configureTracor: (options) => builder.Configuration.BindTracorOptionsDefault(options)
            )
            .AddFileTracorCollectiveSinkDefault(
               configuration: builder.Configuration,
               configure: (fileTracorOptions) => {
                   fileTracorOptions.GetApplicationStopping = static (sp) => sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
               })
            .AddTracorActivityListener(tracorEnabled)
            .AddTracorInstrumentation<ReadAndWriteInstrumentation>()
            .AddTracorLogger()
            ;
        builder.Services.AddSingleton<ControlService>();
        builder.Services.AddHostedService<WriterService>();
        builder.Services.AddHostedService<ReaderService>();
        var host = builder.Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Start App {MachineName}", System.Environment.MachineName);
        var fileTracorCollectiveSink = host.Services.GetRequiredService<FileTracorCollectiveSink>();
        await fileTracorCollectiveSink.FlushAsync();

        await host.RunAsync();

        logger.LogInformation("Stop App {MachineName}", System.Environment.MachineName);
    }
}

public class ControlService {
    private readonly IHostApplicationLifetime _HostApplicationLifetime;
    private bool _WriterDone;
    private bool _ReaderDone;

    public ControlService(
        IHostApplicationLifetime hostApplicationLifetime) {
        this._HostApplicationLifetime = hostApplicationLifetime;
    }

    internal async Task ReaderDone() {
        this._ReaderDone = true;
        if (this._WriterDone) {
            this._HostApplicationLifetime.StopApplication();
        }
    }

    internal async Task WriterDone() {
        this._WriterDone = true;
        if (this._ReaderDone) {
            this._HostApplicationLifetime.StopApplication();
        }
    }
}

public class WriterService : BackgroundService {
    private readonly ControlService _ControlService;
    private readonly FileTracorCollectiveSink _FileTracorCollectiveSink;
    private readonly ILogger<WriterService> _Logger;

    public WriterService(
        ControlService controlService,
        FileTracorCollectiveSink fileTracorCollectiveSink,
        ILogger<WriterService> logger
        ) {
        this._ControlService = controlService;
        this._FileTracorCollectiveSink = fileTracorCollectiveSink;
        this._Logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            for (int outerloop = 0; outerloop < 1000; outerloop++) {
                int sum = 0;
                for (int innerloop = 0; innerloop < 1000; innerloop++) {
                    this._Logger.WriterInnerLoop(innerloop);
                    sum += innerloop;
                }
                this._Logger.WriterInnerSum(sum);
                await this._FileTracorCollectiveSink.FlushAsync();
                await Task.Delay(Program.TimeSpanPeriod);
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

public class ReaderService : BackgroundService {
    private readonly FileTracorCollectiveSink _FileTracorCollectiveSink;
    private readonly ControlService _ControlService;

    public ReaderService(
        FileTracorCollectiveSink fileTracorCollectiveSink,
        ControlService controlService
        ) {
        this._FileTracorCollectiveSink = fileTracorCollectiveSink;
        this._ControlService = controlService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            try {
                string logfile;
                while (true) {
                    var fileFQN = this._FileTracorCollectiveSink.GetCurrentFileFQN();
                    if (fileFQN is { Length: > 0 }) {
                        logfile = fileFQN; break;
                    } else {
                        await Task.Delay(10);
                        continue;
                    }
                }

                await Task.Delay(1000);
                await Task.Delay(Program.TimeSpanPeriod);

                var tracorDataRecordPool = new TracorDataRecordPool(0);
                var jsonSerializerOptions = new JsonSerializerOptions()
                    .AddTracorDataMinimalJsonConverter(tracorDataRecordPool);
                using (var fileStream = System.IO.File.Open(logfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    //int count = 0;
                    //var subResult = await JsonLinesSerializer.DeserializeCallbackRetryAsync<TracorDataRecord>(
                    //    utf8Json: fileStream,
                    //    callback: (item) => { count++; item.Dispose(); },
                    //    retry: static async () => await Task.Delay(TimeSpan.FromMinutes(2)).ConfigureAwait(false),
                    //    options: jsonSerializerOptions,
                    //    leaveOpen: true,
                    //    cancellationToken: CancellationToken.None);
                    //System.Console.WriteLine($"lastPosition:{subResult.lastPosition}");
                    //System.Console.WriteLine($"error:{subResult.error?.Message}");
                    //System.Console.WriteLine($"count:{count}");

                    int count = 0;

                    await TracorDataSerialization.DeserializeMinimalJsonlCallbackAsync(
                        logfile,
                        (item) => {
                            count++;
                            item.Dispose();
                        },
                        jsonSerializerOptions,
                        CancellationToken.None);
                    System.Console.WriteLine($"count:{count}");

                }
            } catch (Exception error) {
                System.Console.Error.WriteLine(error.ToString());
            }
        } finally {
            await this._ControlService.ReaderDone(); ;
        }
    }
}