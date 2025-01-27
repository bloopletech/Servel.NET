global using QueueJob = System.Func<System.IServiceProvider, System.Threading.CancellationToken, System.Threading.Tasks.ValueTask>;
using System.Threading.Channels;

namespace Servel.NET;

public interface IBackgroundTaskQueue
{
    ValueTask QueueAsync(QueueJob workItem);
    ValueTask<QueueJob> DequeueAsync(CancellationToken cancellationToken);
}

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<QueueJob> _queue;

    public BackgroundTaskQueue()
    {
        _queue = Channel.CreateUnbounded<QueueJob>();
    }

    public async ValueTask QueueAsync(QueueJob workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<QueueJob> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}