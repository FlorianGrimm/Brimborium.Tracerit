using BenchmarkDotNet.Running;

namespace BenchmarkPropertyVsGC {
    internal class Program {
        static void Main(string[] args) {
            //(new Benchmarks()).UsingTracorDataProperty();
            var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}

/*

MemoryDiagnoser

| Method                  | Mean     | Error   | StdDev  | Gen0       | Gen1       | Gen2     | Allocated |
|------------------------ |---------:|--------:|--------:|-----------:|-----------:|---------:|----------:|
| UsingGC                 | 137.6 ms | 2.71 ms | 5.79 ms | 64250.0000 | 34000.0000 |        - | 771.14 MB |
| UsingTracorDataProperty | 193.1 ms | 3.85 ms | 9.16 ms |  4333.3333 |  1000.0000 | 333.3333 |  50.42 MB |
 
 
 */