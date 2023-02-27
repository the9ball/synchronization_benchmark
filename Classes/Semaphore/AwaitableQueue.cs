using System.Collections;
using System.Collections.Concurrent;

namespace benchmark.Semaphore;

/// <summary>
/// <see cref="Enqueue(TItem)"/>されるまで待つ<see cref="DequeueAsync(CancellationToken)"/>を持ったキュー実装
/// </summary>
public class AwaitableQueue<TItem> : IReadOnlyCollection<TItem>
{
    private readonly ConcurrentQueue<TItem> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(0);

    /// <inheritdoc cref="ConcurrentQueue{T}.Enqueue(T)"/>
    public void Enqueue(TItem item)
    {
        _queue.Enqueue(item);
        _semaphore.Release(1);
    }

    /// <summary>
    /// <see cref="ConcurrentQueue{T}.TryDequeue(out T)"/>できる状態になるまで待つ
    /// </summary>
    public async ValueTask<TItem> DequeueAsync(CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);

        if (!_queue.TryDequeue(out var item)) throw new InvalidProgramException();
        return item;
    }

    /// <inheritdoc cref="ConcurrentQueue{T}.Enqueue(T)"/>
    public bool TryPeek(out TItem item) => _queue.TryPeek(out item);

    /// <inheritdoc cref="ConcurrentQueue{T}.Count"/>
    public int Count => _queue.Count;

    /// <inheritdoc cref="ConcurrentQueue{T}.IsEmpty"/>
    public bool IsEmpty => _queue.IsEmpty;

    #region Implement IReadOnlyCollection

    int IReadOnlyCollection<TItem>.Count => Count;
    IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => _queue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();

    #endregion
}
