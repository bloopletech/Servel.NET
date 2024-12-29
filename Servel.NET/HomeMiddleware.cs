using Microsoft.Net.Http.Headers;
using Servel.NET.Extensions;
using System.Net.Mime;
using System.Text.Json;

namespace Servel.NET;

public class HomeMiddleware(RequestDelegate next, IEnumerable<Listing> listings)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if(ShouldProcess(httpContext)) await Process(httpContext);
        else await next.Invoke(httpContext);
    }

    private static bool ShouldProcess(HttpContext httpContext) => httpContext.Request.IsGetOrHead() && httpContext.Request.IsRoot();

    private async Task Process(HttpContext httpContext)
    {
        httpContext.Response.Headers.Vary = HeaderNames.Accept;

        if(httpContext.Request.Headers.Accept.Contains(MediaTypeNames.Application.Json))
        {
            await Results.Text(RenderResponse(), MediaTypeNames.Application.Json).ExecuteAsync(httpContext);
            return;
        }

        await Results.Text(Resources.Get("Views", "home.html"), MediaTypeNames.Text.Html).ExecuteAsync(httpContext);
    }

    private byte[] RenderResponse()
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            listings.Select(l => new ListingEntry(l.UrlPath, l.Name)),
            SerializationSourceGenerationContext.Default.IEnumerableListingEntry);
    }
}
