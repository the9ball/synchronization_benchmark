using BenchmarkDotNet.Attributes;
using System.Runtime.ExceptionServices;

[MemoryDiagnoser]
public class Runner
{
    /// <summary>
    /// <see cref="Benchmark(SynchronizationContext)"/>で実行するループの回数
    /// </summary>
    const int LoopCount = 100;

    /// <summary>
    /// <see cref="Benchmark(SynchronizationContext)"/>で実行するループ中にPostする回数
    /// </summary>
    const int ConcurrencyRequest = 10;

    [Benchmark]
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
        static Task awaitPost(SynchronizationContext synchronizationContext)
        {
            var tcs = new TaskCompletionSource();
            synchronizationContext.Post(static x => ((TaskCompletionSource)x!).SetResult(), tcs);
            return tcs.Task;
        }

        for (int j = 0; j < LoopCount; j++)
        {
            // Delayのケースのワーストを測るためPostしたものが実行されるタイミングから開始する
            await awaitPost(synchronizationContext);

            for (int i = 0; i < ConcurrencyRequest; i++)
            {
                synchronizationContext.Post(static _ => Thread.Sleep(1), null);
            }
        }

        // 最後のPostが終わるまで待つ
        await awaitPost(synchronizationContext);
    }

    private static void OnError(Exception ex)
        => ExceptionDispatchInfo.Capture(ex).Throw();
}
