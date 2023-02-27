using BenchmarkDotNet.Attributes;
using System.Runtime.ExceptionServices;

[MemoryDiagnoser]
public class Runner
{
    /// <summary>
    /// <see cref="Benchmark(SynchronizationContext)"/>で実行するループの回数
    /// </summary>
    const int LoopCount = 1000;

    /// <summary>
    /// <see cref="SynchronizationContext.Post(SendOrPostCallback, object?)"/>に渡すための参照型
    /// </summary>
    class Holder
    {
        public int Value { get; }
        public Holder(int v) => Value = v;
    }

    private Holder[] _delays;

    public Runner()
    {
        var random = new Random();
        const int min = 100;
        _delays = Enumerable.Range(min, LoopCount)
            .Select(x => new Holder(x / min)) // 最低1になるはず
            .OrderBy(_ => random.Next()) // 適当に混ぜる
            .ToArray();
    }

    [Benchmark]
    public Task Delay() => Benchmark(new benchmark.Delay.SequentialSynchronizationContext(TimeSpan.FromMilliseconds(100), OnError));

    [Benchmark]
    public Task Semaphore() => Benchmark(new benchmark.Semaphore.SequentialSynchronizationContext(OnError));

    [Benchmark]
    public Task Channel() => Benchmark(new benchmark.Channels.SequentialSynchronizationContext(OnError));


    private async Task Benchmark(SynchronizationContext synchronizationContext)
    {
        static Task awaitPost(SynchronizationContext synchronizationContext)
        {
            var tcs = new TaskCompletionSource();
            synchronizationContext.Post(static x => ((TaskCompletionSource)x!).SetResult(), tcs);
            return tcs.Task;
        }

        // Delayのケースのワーストを測るためPostしたものが実行されるタイミングから開始する
        await awaitPost(synchronizationContext);

        foreach (var i in _delays)
        {
            synchronizationContext.Post(static x => Thread.Sleep(((Holder)x!).Value), i);
        }

        // 最後のPostが終わるまで待つ
        await awaitPost(synchronizationContext);
    }

    private static void OnError(Exception ex)
        => ExceptionDispatchInfo.Capture(ex).Throw();
}
