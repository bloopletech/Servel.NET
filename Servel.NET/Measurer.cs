using System.Diagnostics;

namespace Servel.NET;

public interface IMeasurer : IDisposable
{
    IMeasurer Measure(string subContext);
    void Measure(string subContext, Action callback);
    T Measure<T>(string subContext, Func<T> callback);
    Task Measure(string subContext, Func<Task> callback);
    Task<T> Measure<T>(string subContext, Func<Task<T>> callback);
}

public class Measurer(ILogger logger, IDisposable? loggerScope, params string[] contexts) : IMeasurer
{
    private readonly long start = Stopwatch.GetTimestamp();
    private bool disposed;

    protected virtual void Dispose(bool disposing)
    {
        if(!disposed)
        {
            if(disposing)
            {
                var duration = (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                if(contexts.Length > 0)
                {
                    logger.LogInformation("{Contexts} took {Elapsed}ms", string.Join(":", contexts), duration);
                }
                else
                {
                    logger.LogInformation("Took {Elapsed}ms", duration);
                }
                loggerScope?.Dispose();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public IMeasurer Measure(string subContext)
    {
        return new Measurer(logger, null, [..contexts, subContext]);
    }

    public void Measure(string subContext, Action callback)
    {
        using(Measure(subContext)) callback();
    }

    public T Measure<T>(string subContext, Func<T> callback)
    {
        using(Measure(subContext)) return callback();
    }

    public async Task Measure(string subContext, Func<Task> callback)
    {
        using(Measure(subContext)) await callback();
    }

    public async Task<T> Measure<T>(string subContext, Func<Task<T>> callback)
    {
        using(Measure(subContext)) return await callback();
    }
}

public class NullMeasurer : IMeasurer
{
    public virtual void Dispose()
    {
    }

    public IMeasurer Measure(string subContext)
    {
        return new NullMeasurer();
    }

    public void Measure(string subContext, Action callback)
    {
        callback();
    }

    public T Measure<T>(string subContext, Func<T> callback)
    {
        return callback();
    }

    public async Task Measure(string subContext, Func<Task> callback)
    {
        await callback();
    }

    public async Task<T> Measure<T>(string subContext, Func<Task<T>> callback)
    {
        return await callback();
    }
}

public class LoggerScopeNullMeasurer(IDisposable? loggerScope) : NullMeasurer
{
    private bool disposed;

    protected virtual void Dispose(bool disposing)
    {
        if(!disposed)
        {
            if(disposing) loggerScope?.Dispose();
            disposed = true;
        }
    }

    public override void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
