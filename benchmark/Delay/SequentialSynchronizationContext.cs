using System.Collections.Concurrent;

namespace benchmark.Delay;

/// <summary>
/// 直列で処理されることを保証する<see cref="SynchronizationContext"/>
/// <see cref="Post(SendOrPostCallback, object?)"/>されたもの同士が並行して実行されることはない
/// </summary>
public class SequentialSynchronizationContext : SynchronizationContext, IAsyncDisposable
{
    private readonly ConcurrentQueue<(SendOrPostCallback callback, object? state)> _queue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;
    private readonly Action<Exception> _onError;

    /// <summary>
    /// 処理ループも起動する
    /// </summary>
    public SequentialSynchronizationContext(TimeSpan interval, Action<Exception> onError)
    {
        _onError = onError;

        _loop = Loop(interval, _cts.Token);
    }

    private async Task Loop(TimeSpan interval, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (_queue.TryDequeue(out var item))
                {
                    var (callback, state) = item;
                    callback?.Invoke(state);
                }
                else
                {
                    // Postされていなければinterval分待つ
                    await Task.Delay(interval);
                }
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

