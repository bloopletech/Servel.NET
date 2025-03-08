using Microsoft.Extensions.Caching.Memory;
using Servel.NET.Extensions;

namespace Servel.NET;

public class BlockFailedAuthenticationMiddleware(RequestDelegate next, IMemoryCache cache)
{
    private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
    private const int MaxFailures = 10;
    private static readonly TimeSpan TrackFailuresSlidingDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan BlockDuration = TimeSpan.FromHours(1);

    private static string BlocksCacheKey(HttpContext httpContext) =>
        $"Blocks/{httpContext.SiteId()}/{httpContext.Connection.RemoteIpAddress}";
    private static string FailuresCacheKey(HttpContext httpContext) =>
        $"Failures/{httpContext.SiteId()}/{httpContext.Connection.RemoteIpAddress}";

    public async Task Invoke(HttpContext httpContext)
    {
        var clientIp = httpContext.Connection.RemoteIpAddress;
        if(clientIp == null || cache.Get(BlocksCacheKey(httpContext)) != null)
        {
            await Results.Unauthorized().ExecuteAsync(httpContext);
            return;
        }

        await next.Invoke(httpContext);

        if(httpContext.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            var failures = await IncrementFailures(httpContext);
            if(failures > MaxFailures)
            {
                cache.Set(BlocksCacheKey(httpContext), new object(), BlockDuration);
                cache.Remove(FailuresCacheKey(httpContext));
            }
        }
        else
        {
            cache.Remove(FailuresCacheKey(httpContext));
        }
    }

    private async Task<int> IncrementFailures(HttpContext httpContext)
    {
        int failures;
        await semaphoreSlim.WaitAsync();
        try
        {
            failures = cache.GetOrCreate(FailuresCacheKey(httpContext), (cacheEntry) => 0);
            failures++;
            cache.Set(FailuresCacheKey(httpContext), failures, TrackFailuresSlidingDuration);
        }
        finally
        {
            semaphoreSlim.Release();
        }

        return failures;
    }
}
