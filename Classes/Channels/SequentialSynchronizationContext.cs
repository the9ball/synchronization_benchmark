using System.Threading.Channels;

namespace benchmark.Channels;

/// <summary>
/// 直列で処理されることを保証する<see cref="SynchronizationContext"/>
/// <see cref="Post(SendOrPostCallback, object?)"/>されたもの同士が並行して実行されることはない
/// </summary>
public class SequentialSynchronizationContext : SynchronizationContext, IAsyncDisposable
{
    private readonly ChannelWriter<(SendOrPostCallback callback, object? state)> _channelWriter;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;
    private readonly Action<Exception> _onError;

    /// <summary>
    /// 処理ループも起動する
    /// </summary>
    public SequentialSynchronizationContext(Action<Exception> onError)
    {
        _onError = onError;

        var channel = Channel.CreateUnbounded<(SendOrPostCallback callback, object? state)>(new UnboundedChannelOptions { SingleReader = true });
        _channelWriter = channel.Writer;

        _loop = Loop(channel.Reader, _cts.Token);
    }

    private async Task Loop(ChannelReader<(SendOrPostCallback callback, object? state)> reader, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var (callback, state) = await reader.ReadAsync(ct);
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
        if (!_channelWriter.TryWrite((d, state))) throw new InvalidOperationException();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        await _loop;
        _cts.Dispose();

        _channelWriter.Complete();
    }
}
