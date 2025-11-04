using BenchmarkDotNet.Running;

namespace BenchmarkPropertyVsGC {
    internal class Program {
        static void Main(string[] args) {
#if false
            Benchmarks benchmarks = new();
            benchmarks.Setup();
            benchmarks.TracorActivityAndLogger();
            benchmarks.TracorActivityAndTracor();
            //benchmarks.UsingGC();
            benchmarks.UsingTracorDataProperty();
#else
            _ = BenchmarkRunner.Run(typeof(Program).Assembly);
#endif
        }
    }
}

/*

| Method                  | Mean       | Error    | StdDev   |
|------------------------ |-----------:|---------:|---------:|
| UsingGC                 | 1,327.3 ms | 22.65 ms | 21.19 ms |
| UsingTracorDataProperty |   650.6 ms | 12.92 ms | 23.63 ms |
| TracorActivityAndLogger |   207.4 ms |  4.10 ms |  7.60 ms |
| TracorActivityAndTracor |   159.0 ms |  3.17 ms |  6.33 ms |

*/