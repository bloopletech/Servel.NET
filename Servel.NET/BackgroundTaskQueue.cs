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

    public BackgroundTaskQueue(int capacity)
    {
        // Capacity should be set based on the expected application load and
        // number of concurrent threads accessing the queue.            
        // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task,
        // which completes only when space became available. This leads to backpressure,
        // in case too many publishers/calls start accumulating.
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<QueueJob>(options);
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