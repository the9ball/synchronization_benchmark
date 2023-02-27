using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using System.Runtime.ExceptionServices;

[EtwProfiler]
public class Runner
{
    [Benchmark(Baseline = true)]
    public async Task Delay()
    {
        var s = new benchmark.Delay.SequentialSynchronizationContext(TimeSpan.FromMilliseconds(100), OnError);
        await Benchmark(s);
        await s.DisposeAsync();
    }

    [Benchmark]
    public async Task DelayWithLock()
    {
        var s = new benchmark.DelayLock.SequentialSynchronizationContext(TimeSpan.FromMilliseconds(100), OnError);
        await Benchmark(s);
        await s.DisposeAsync();
    }

    [Benchmark]
    public async Task Semaphore()
    {
        var s = new benchmark.Semaphore.SequentialSynchronizationContext(OnError);
        await Benchmark(s);
        await s.DisposeAsync();
    }

    [Benchmark]
    public async Task Channel()
    {
        var s = new benchmark.Channels.SequentialSynchronizationContext(OnError);
        await Benchmark(s);
        await s.DisposeAsync();
    }


    private async Task Benchmark(SynchronizationContext synchronizationContext)
    {
        await Task.Delay(TimeSpan.FromSeconds(10));
    }

    private static void OnError(Exception ex)
        => ExceptionDispatchInfo.Capture(ex).Throw();
}
