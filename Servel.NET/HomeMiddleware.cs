using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;

namespace Servel.NET;

public class HomeMiddleware(RequestDelegate next, IEnumerable<Listing> listings)
{
    private readonly RequestDelegate _next = next;
    private readonly IEnumerable<Listing> _listings = listings;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (ShouldProcess(httpContext)) await Process(httpContext);
        else await _next.Invoke(httpContext);
    }

    private static bool ShouldProcess(HttpContext httpContext) => FileHelpers.IsGetOrHeadMethod(httpContext.Request.Method)
        && httpContext.Request.Path == "/";

    private async Task Process(HttpContext httpContext)
    {
        httpContext.Response.Headers.Vary = HeaderNames.Accept;

        if (httpContext.Request.Headers.Accept.Contains(MediaTypeNames.Application.Json))
        {
            await Results.Text(RenderResponse(), MediaTypeNames.Application.Json).ExecuteAsync(httpContext);
            return;
        }

        await Results.Text(StaticResources.GetView("home.html"), MediaTypeNames.Text.Html).ExecuteAsync(httpContext);
    }

    private byte[] RenderResponse()
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            _listings.Select(l => new ListingEntry(l.UrlPath, l.Name)),
            SerializationSourceGenerationContext.Default.IEnumerableListingEntry);
    }
}
