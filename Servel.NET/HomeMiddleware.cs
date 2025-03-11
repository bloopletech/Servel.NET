using Microsoft.Net.Http.Headers;
using Servel.NET.Extensions;
using System.Net.Mime;
using System.Text.Json;

namespace Servel.NET;

public class HomeMiddleware(RequestDelegate next, IEnumerable<Listing> listings) : MiddlewareBase(next)
{
    public override bool ShouldRun() => Request.IsGetOrHead() && Request.IsRoot();

    public override async Task RunAsync()
    {
        Response.Headers.Vary = HeaderNames.Accept;

        if(Request.Headers.Accept.Contains(MediaTypeNames.Application.Json))
        {
            await Results.Text(RenderResponse(), MediaTypeNames.Application.Json).ExecuteAsync(HttpContext);
            return;
        }

        await Results.Text(Resources.Get("Views", "home.html"), MediaTypeNames.Text.Html).ExecuteAsync(HttpContext);
    }

    private byte[] RenderResponse()
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            listings.Select(l => new ListingEntry(l.UrlPath, l.Name)),
            SerializationSourceGenerationContext.Default.IEnumerableListingEntry);
    }
}
