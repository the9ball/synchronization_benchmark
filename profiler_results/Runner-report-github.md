``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1265/22H2/2022Update/SunValley2)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.200
  [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2


```
|        Method |    Mean |   Error |  StdDev | Ratio |
|-------------- |--------:|--------:|--------:|------:|
|         Delay | 10.07 s | 0.054 s | 0.050 s |  1.00 |
| DelayWithLock | 10.02 s | 0.004 s | 0.003 s |  0.99 |
|     Semaphore | 10.01 s | 0.006 s | 0.006 s |  0.99 |
|       Channel | 10.01 s | 0.005 s | 0.005 s |  0.99 |
