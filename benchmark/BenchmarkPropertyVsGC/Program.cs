using BenchmarkDotNet.Running;

namespace BenchmarkPropertyVsGC {
    internal class Program {
        static void Main(string[] args) {
#if false
            Benchmarks benchmarks = new();
            benchmarks.Setup();
            benchmarks.TracorActivityAndLogger();
            benchmarks.TracorActivityAndTracor();
            // benchmarks.UsingGC();
            // benchmarks.UsingTracorDataProperty();
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

| Method                  | Mean       | Error    | StdDev   |
|------------------------ |-----------:|---------:|---------:|
| UsingGC                 | 1,439.0 ms | 27.41 ms | 31.56 ms |
| UsingTracorDataProperty |   796.7 ms | 15.88 ms | 28.63 ms |
| TracorActivityAndLogger |   268.3 ms |  4.98 ms |  9.48 ms |
| TracorActivityAndTracor |   196.6 ms |  3.84 ms |  7.03 ms |



| Method                  | Job              | Force | Server | Mean     | Error     | StdDev   | Gen0       | Allocated |
|------------------------ |----------------- |------ |------- |---------:|----------:|---------:|-----------:|----------:|
| TracorActivityAndTracor | WorkstationForce | False | False  | 148.0 ms |  72.42 ms |  3.97 ms | 39500.0000 | 473.99 MB |
| TracorActivityAndTracor | Workstation      | True  | False  | 148.4 ms |  78.66 ms |  4.31 ms | 39500.0000 | 473.99 MB |
| TracorActivityAndTracor | ServerForce      | True  | True   | 207.0 ms | 319.28 ms | 17.50 ms | 24500.0000 | 473.99 MB |
| TracorActivityAndLogger | Workstation      | True  | False  | 218.5 ms | 161.21 ms |  8.84 ms | 30000.0000 | 359.34 MB |
| TracorActivityAndLogger | WorkstationForce | False | False  | 221.0 ms |  69.95 ms |  3.83 ms | 30000.0000 | 359.34 MB |
| TracorActivityAndLogger | Server           | False | True   | 283.9 ms | 375.12 ms | 20.56 ms | 48500.0000 | 359.34 MB |
| TracorActivityAndLogger | ServerForce      | True  | True   | 284.4 ms | 232.72 ms | 12.76 ms | 18500.0000 | 359.34 MB |
| TracorActivityAndTracor | Server           | False | True   | 343.3 ms | 728.08 ms | 39.91 ms | 24500.0000 | 473.99 MB |

| Method                  | Job              | Force | Server | Mean     | Error     | StdDev   | Gen0       | Gen1     | Gen2     | Allocated |
|------------------------ |----------------- |------ |------- |---------:|----------:|---------:|-----------:|---------:|---------:|----------:|
| TracorActivityAndTracor | WorkstationForce | False | False  | 181.9 ms |  76.93 ms |  4.22 ms | 39333.3333 |        - |        - | 473.99 MB |
| TracorActivityAndTracor | Workstation      | True  | False  | 186.7 ms | 106.90 ms |  5.86 ms | 39333.3333 |        - |        - | 473.99 MB |
| TracorActivityAndTracor | Server           | False | True   | 209.7 ms | 161.04 ms |  8.83 ms | 28000.0000 | 333.3333 | 333.3333 | 473.99 MB |
| TracorActivityAndTracor | ServerForce      | True  | True   | 225.0 ms | 105.68 ms |  5.79 ms | 24333.3333 |        - |        - | 473.99 MB |
| TracorActivityAndLogger | Workstation      | True  | False  | 241.6 ms |  87.58 ms |  4.80 ms | 30000.0000 |        - |        - | 359.34 MB |
| TracorActivityAndLogger | WorkstationForce | False | False  | 241.8 ms |  82.64 ms |  4.53 ms | 30000.0000 |        - |        - | 359.34 MB |
| TracorActivityAndLogger | Server           | False | True   | 285.5 ms | 260.77 ms | 14.29 ms | 48500.0000 |        - |        - | 359.34 MB |
| TracorActivityAndLogger | ServerForce      | True  | True   | 317.2 ms | 202.82 ms | 11.12 ms | 18500.0000 |        - |        - | 359.34 MB |
 
 */