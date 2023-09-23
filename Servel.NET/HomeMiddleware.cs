namespace Servel.NET;

public class HomeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<Listing> _listings;

    public HomeMiddleware(RequestDelegate next, IEnumerable<Listing> listings)
    {
        _next = next;
        _listings = listings;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (ShouldProcess(httpContext)) await Process(httpContext);
        else await _next.Invoke(httpContext);
    }

    private bool ShouldProcess(HttpContext httpContext) => FileHelpers.IsGetOrHeadMethod(httpContext.Request.Method)
        && httpContext.Request.Path == "/";

    private async Task Process(HttpContext httpContext)
    {
        await Results.Extensions.View("Home", new { Listings = _listings }).ExecuteAsync(httpContext);
    }
}
