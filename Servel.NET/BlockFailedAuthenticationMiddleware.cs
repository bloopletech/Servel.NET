using Microsoft.Extensions.Caching.Memory;
using Servel.NET.Extensions;

namespace Servel.NET;

public class BlockFailedAuthenticationMiddleware(RequestDelegate next, IMemoryCache cache) : MiddlewareBase(next)
{
    private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
    private const int MaxFailures = 10;
    private static readonly TimeSpan TrackFailuresSlidingDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan BlockDuration = TimeSpan.FromHours(1);

    private string BlocksCacheKey => $"Blocks/{HttpContext.SiteId()}/{Connection.RemoteIpAddress}";
    private string FailuresCacheKey => $"Failures/{HttpContext.SiteId()}/{Connection.RemoteIpAddress}";

    public override IResult? Before()
    {
        var clientIp = Connection.RemoteIpAddress;
        if(clientIp == null || cache.Get(BlocksCacheKey) != null) return Results.Unauthorized();
        return null;
    }

    public override async Task AfterAsync()
    {
        if(Response.StatusCode != StatusCodes.Status401Unauthorized)
        {
            cache.Remove(FailuresCacheKey);
            return;
        }

        var failures = await IncrementFailures();
        if(failures > MaxFailures)
        {
            cache.Set(BlocksCacheKey, new object(), BlockDuration);
            cache.Remove(FailuresCacheKey);
        }
    }

    private async Task<int> IncrementFailures()
    {
        int failures;
        await semaphoreSlim.WaitAsync();
        try
        {
            failures = cache.GetOrCreate(FailuresCacheKey, (cacheEntry) => 0);
            failures++;
            cache.Set(FailuresCacheKey, failures, TrackFailuresSlidingDuration);
        }
        finally
        {
            semaphoreSlim.Release();
        }

        return failures;
    }
}
