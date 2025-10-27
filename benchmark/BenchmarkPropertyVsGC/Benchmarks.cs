using Brimborium.Tracerit;
using CommandLine;
using Microsoft.VSDiagnostics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
namespace BenchmarkPropertyVsGC {
    // For more information on the VS BenchmarkDotNet Diagnosers see https://learn.microsoft.com/visualstudio/profiling/profiling-with-benchmark-dotnet
    /*
    [CPUUsageDiagnoser]
    */
    
    [Config(typeof(Config))]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    
    public class Benchmarks {
        private class Config : ManualConfig {
            public Config() {
                AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(true).WithId("ServerForce"));
                AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(false).WithId("Server"));
                AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(true).WithId("Workstation"));
                AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(false).WithId("WorkstationForce"));
            }
        }

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
    }
}