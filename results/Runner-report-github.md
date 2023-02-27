``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1265/22H2/2022Update/SunValley2)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.200
  [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2


```
|        Method |    Mean |   Error |  StdDev | Allocated |
|-------------- |--------:|--------:|--------:|----------:|
|         Delay | 15.68 s | 0.012 s | 0.011 s |  11.77 KB |
| DelayWithLock | 15.69 s | 0.009 s | 0.008 s |  10.93 KB |
|     Semaphore | 15.58 s | 0.007 s | 0.006 s |  12.14 KB |
|       Channel | 15.57 s | 0.006 s | 0.006 s |  11.92 KB |
