using BenchmarkDotNet.Running;

namespace BenchmarkPropertyVsGC {
    internal class Program {
        static void Main(string[] args) {
#if true
            Benchmarks benchmarks = new();
            benchmarks.Setup();
            benchmarks.TracorActivityAndLogger();
            benchmarks.TracorActivityAndTracor();
#else
            _ = BenchmarkRunner.Run(typeof(Program).Assembly);
#endif
        }
    }
}

/*

MemoryDiagnoser

| Method                  | Mean     | Error   | StdDev  | Gen0       | Gen1       | Gen2     | Allocated |
|------------------------ |---------:|--------:|--------:|-----------:|-----------:|---------:|----------:|
| UsingGC                 | 137.6 ms | 2.71 ms | 5.79 ms | 64250.0000 | 34000.0000 |        - | 771.14 MB |
| UsingTracorDataProperty | 193.1 ms | 3.85 ms | 9.16 ms |  4333.3333 |  1000.0000 | 333.3333 |  50.42 MB |
 

| Method                  | Job              | Force | Server | Mean    | Error    | StdDev   | Gen0        | Gen1        | Gen2      | Allocated  |
|------------------------ |----------------- |------ |------- |--------:|---------:|---------:|------------:|------------:|----------:|-----------:|
| UsingGC                 | WorkstationForce | False | False  | 1.307 s | 0.0213 s | 0.0312 s | 644000.0000 | 342000.0000 |         - | 7711.41 MB |
| UsingGC                 | Workstation      | True  | False  | 1.337 s | 0.0269 s | 0.0402 s | 644000.0000 | 342000.0000 |         - | 7711.41 MB |
| UsingTracorDataProperty | WorkstationForce | False | False  | 1.647 s | 0.1001 s | 0.1467 s |  39000.0000 |           - |         - |  467.76 MB |
| UsingTracorDataProperty | Workstation      | True  | False  | 1.931 s | 0.0250 s | 0.0375 s |  39000.0000 |   1000.0000 |         - |  467.76 MB |
| UsingTracorDataProperty | ServerForce      | True  | True   | 1.996 s | 0.0339 s | 0.0487 s |  22000.0000 |   1000.0000 |         - |  467.76 MB |
| UsingTracorDataProperty | Server           | False | True   | 2.020 s | 0.0528 s | 0.0773 s |  62000.0000 |   1000.0000 | 1000.0000 |  467.76 MB |
| UsingGC                 | Server           | False | True   | 2.710 s | 0.0838 s | 0.1254 s | 350000.0000 | 103000.0000 |         - | 7711.41 MB |
| UsingGC                 | ServerForce      | True  | True   | 2.735 s | 0.1115 s | 0.1634 s | 350000.0000 |  99000.0000 |         - | 7711.41 MB |



| Method                  | Mean       | Error    | StdDev   |
|------------------------ |-----------:|---------:|---------:|
| UsingGC                 | 1,384.9 ms | 27.49 ms | 38.54 ms |
| UsingTracorDataProperty |   930.7 ms | 18.47 ms | 36.46 ms |


| Method                  | Job              | Force | Server | Mean       | Error     | StdDev    | Gen0        | Gen1        | Gen2      | Allocated |
|------------------------ |----------------- |------ |------- |-----------:|----------:|----------:|------------:|------------:|----------:|----------:|
| UsingTracorDataProperty | Workstation      | True  | False  |   948.5 ms |  16.03 ms |  24.00 ms | 230000.0000 |   1000.0000 |         - |   2.69 GB |
| UsingTracorDataProperty | WorkstationForce | False | False  |   963.2 ms |  19.22 ms |  28.77 ms | 230000.0000 |   1000.0000 |         - |   2.69 GB |
| UsingGC                 | WorkstationForce | False | False  | 1,283.9 ms |  32.70 ms |  47.94 ms | 644000.0000 | 342000.0000 |         - |   7.53 GB |
| UsingGC                 | Workstation      | True  | False  | 1,310.5 ms |  25.70 ms |  37.68 ms | 644000.0000 | 342000.0000 |         - |   7.53 GB |
| UsingTracorDataProperty | ServerForce      | True  | True   | 1,563.8 ms | 132.48 ms | 198.29 ms | 132000.0000 |   1000.0000 |         - |   2.69 GB |
| UsingTracorDataProperty | Server           | False | True   | 1,651.5 ms |  55.66 ms |  81.58 ms | 133000.0000 |   7000.0000 | 1000.0000 |   2.69 GB |
| UsingGC                 | Server           | False | True   | 2,635.9 ms | 144.70 ms | 216.57 ms | 350000.0000 | 100000.0000 |         - |   7.53 GB |
| UsingGC                 | ServerForce      | True  | True   | 2,687.7 ms | 124.63 ms | 186.54 ms | 349000.0000 | 102000.0000 |         - |   7.53 GB | 
 
 
| Method                  | Mean       | Error    | StdDev   |
|------------------------ |-----------:|---------:|---------:|
| UsingGC                 | 1,386.4 ms | 27.39 ms | 54.70 ms |
| UsingTracorDataProperty |   628.1 ms | 12.41 ms | 16.99 ms |


| Method                  | Job              | Force | Server | Mean       | Error     | StdDev    | Gen0        | Gen1        | Allocated  |
|------------------------ |----------------- |------ |------- |-----------:|----------:|----------:|------------:|------------:|-----------:|
| UsingTracorDataProperty | Workstation      | True  | False  |   630.7 ms |   8.11 ms |  12.14 ms |  38000.0000 |   1000.0000 |  466.29 MB |
| UsingTracorDataProperty | WorkstationForce | False | False  |   633.1 ms |   5.76 ms |   8.44 ms |  38000.0000 |           - |  466.29 MB |
| UsingTracorDataProperty | ServerForce      | True  | True   |   743.8 ms |  18.97 ms |  28.39 ms |  21000.0000 |   1000.0000 |  466.29 MB |
| UsingTracorDataProperty | Server           | False | True   |   751.4 ms |  15.19 ms |  22.26 ms |  23000.0000 |           - |  466.29 MB |
| UsingGC                 | WorkstationForce | False | False  | 1,274.7 ms |  25.84 ms |  38.67 ms | 644000.0000 | 342000.0000 | 7711.41 MB |
| UsingGC                 | Workstation      | True  | False  | 1,326.3 ms |  27.20 ms |  40.71 ms | 644000.0000 | 342000.0000 | 7711.41 MB |
| UsingGC                 | ServerForce      | True  | True   | 2,341.1 ms | 123.21 ms | 184.41 ms | 350000.0000 | 104000.0000 | 7711.41 MB |
| UsingGC                 | Server           | False | True   | 2,529.2 ms |  55.75 ms |  81.72 ms | 350000.0000 | 102000.0000 | 7711.41 MB |
 */