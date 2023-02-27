using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Running;

BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args, DefaultConfig.Instance.AddDiagnoser(new EtwProfiler()));
