#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1822 // Mark members as static

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Brimborium.Tracerit;
using Brimborium.Tracerit.Diagnostics;
using Brimborium.Tracerit.Expression;
using Brimborium.Tracerit.Filter;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using EnvDTE80;
using System.Diagnostics;
using System.Collections.Generic;

namespace BenchmarkPropertyVsGC;
// For more information on the VS BenchmarkDotNet Diagnosers see https://learn.microsoft.com/visualstudio/profiling/profiling-with-benchmark-dotnet
#if false
[CPUUsageDiagnoser]
#endif
#if true
[Config(typeof(Config))]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
#endif
#if false
[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.ColdStart, launchCount: 1, warmupCount: 1, iterationCount: 1, invocationCount: 1)]
#endif
public class Benchmarks {

    private class Config : ManualConfig {
        public Config() {
#if false
            this.AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(true).WithId("ServerForce"));
            this.AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(false).WithId("Server"));
            this.AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(true).WithId("Workstation"));
            this.AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(false).WithId("WorkstationForce"));
#else
            this.AddJob(Job.ShortRun.WithGcServer(true).WithGcForce(true).WithId("ServerForce"));
            this.AddJob(Job.ShortRun.WithGcServer(true).WithGcForce(false).WithId("Server"));
            this.AddJob(Job.ShortRun.WithGcServer(false).WithGcForce(true).WithId("Workstation"));
            this.AddJob(Job.ShortRun.WithGcServer(false).WithGcForce(false).WithId("WorkstationForce"));
#endif
        }
    }
#if false

    [Benchmark]
    public void UsingGC() {
        for (long idxRepeat = 0; idxRepeat < 10000; idxRepeat++) {
            List<List<KeyValuePair<string, object>>> listRepeat = new();
            for (long idxCalls = 0; idxCalls < 1000; idxCalls++) {
                List<KeyValuePair<string, object>> list = new();
                for (long idxProp = 0; idxProp < 10; idxProp++) {
                    list.Add(new KeyValuePair<string, object>("Test", idxProp));
                }
                listRepeat.Add(list);
            }
        }
    }

    [Benchmark]
    public void UsingTracorDataProperty() {
        TracorDataRecordPool pool = new(0);
        for (long idxRepeat = 0; idxRepeat < 10000; idxRepeat++) {
            using (TracorDataRecordCollection collection = new TracorDataRecordCollection()) {
                for (long idxCalls = 0; idxCalls < 1000; idxCalls++) {
                    var tdr = pool.Rent();
                    List<KeyValuePair<string, object>> list = new();
                    for (long idxProp = 0; idxProp < 10; idxProp++) {
                        tdr.ListProperty.Add(TracorDataProperty.CreateIntegerValue("Test", idxProp));
                    }
                    collection.ListData.Add(tdr);
                }
            }
        }
    }
#endif
#if true

    private IHost _Host;

    [GlobalSetup]
    public void Setup() {
        HostApplicationBuilder builder = new([]);
        builder.Logging.ClearProviders();
        _ = builder.Logging.AddConfiguration(
            builder.Configuration.GetSection("Logging"));
        var tracorOptions = builder.Configuration.BindTracorOptionsDefault(new());
        const bool tracorEnabled = true;
        builder.Services.AddTracor(
                addEnabledServices: tracorEnabled,
                configureTracor: (options) => builder.Configuration.BindTracorOptionsDefault(options)
            )
            .AddTracorActivityListener(tracorEnabled)
            .AddTracorInstrumentation<BenchmarkInstrumentation>()
            .AddTracorLogger()
            .AddTracorScopedFilter((tracorScopedFilterBuilder) => {
                tracorScopedFilterBuilder.AddTracorScopedFilterBoth();
                tracorScopedFilterBuilder.AddTracorScopedFilterConfiguration(
                    builder.Configuration.GetSection("Logging"));
            })
            ;

        var host = builder.Build();
        host.Services.TracorActivityListenerStart();
        this._Host = host;
    }
    [GlobalCleanup]
    public static void Cleanup() {
    }

    [Benchmark]
    public void TracorActivityAndLogger() {
        if (this._Host is not { } host) { return; }
        var logger = host.Services.GetRequiredService<ILogger<Benchmarks>>();
        var benchmarkInstrumentation = host.Services.GetRequiredService<BenchmarkInstrumentation>();
        for (long idxRepeat = 0; idxRepeat < 1000; idxRepeat++) {
            using (var activity = benchmarkInstrumentation.StartRoot()) {
                for (long idxCalls = 0; idxCalls < 1000; idxCalls++) {
                    logger.LogIt(idxCalls);
                }
            }
        }
    }

    [Benchmark]
    public void TracorActivityAndTracor() {
        if (this._Host is not { } host) { return; }
        var tracor = host.Services.GetRequiredService<ITracorSink<Benchmarks>>();
        var benchmarkInstrumentation = host.Services.GetRequiredService<BenchmarkInstrumentation>();
        for (long idxRepeat = 0; idxRepeat < 1000; idxRepeat++) {
            using (var activity = benchmarkInstrumentation.StartRoot()) {
                for (long idxCalls = 0; idxCalls < 1000; idxCalls++) {
                    tracor.GetPublicTracor(LogLevel.Information, "gna").TracePublic(idxCalls);
                }
            }
        }
    }

#endif
}

public static partial class BenchmarksLoggerExtensions {
    [LoggerMessage(Level = LogLevel.Information, Message = "Log {value}")]
    public static partial void LogIt(this ILogger logger, long value);
}

public sealed class BenchmarkInstrumentation : InstrumentationBase {
    internal const string ActivitySourceName = "Benchmark";
    internal const string ActivitySourceVersion = "1.0.0";

    public BenchmarkInstrumentation()
        : base(ActivitySourceName, ActivitySourceVersion) {
    }
}
