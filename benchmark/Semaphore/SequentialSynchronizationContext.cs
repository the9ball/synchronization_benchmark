namespace benchmark.Semaphore;

/// <summary>
/// 直列で処理されることを保証する<see cref="SynchronizationContext"/>
/// <see cref="Post(SendOrPostCallback, object?)"/>されたもの同士が並行して実行されることはない
/// </summary>
public class SequentialSynchronizationContext : SynchronizationContext, IAsyncDisposable
{
    private readonly AwaitableQueue<(SendOrPostCallback callback, object? state)> _queue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;
    private readonly Action<Exception> _onError;

    /// <summary>
    /// 処理ループも起動する
    /// </summary>
    public SequentialSynchronizationContext(Action<Exception> onError)
    {
        _onError = onError;

        _loop = Loop(_cts.Token);
    }

    private async Task Loop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var (callback, state) = await _queue.DequeueAsync(ct).ConfigureAwait(false);
                callback?.Invoke(state);
            }
            catch (OperationCanceledException) { } // キャンセル例外は握りつぶす
            catch (Exception ex)
            {
                _onError(ex);
            }
        }
    }

    /// <inheritdoc/>
    public override void Post(SendOrPostCallback d, object? state)
    {
        _queue.Enqueue((d, state));
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        await _loop;
        _cts.Dispose();
    }

    /// <summary>
    /// キューが空かどうか
    /// </summary>
    public bool IsEmpty => _queue.IsEmpty;

    /// <summary>
    /// キューに残っているタスク数
    /// </summary>
    public int Count => _queue.Count;
}

