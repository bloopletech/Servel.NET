using Microsoft.Net.Http.Headers;
using Servel.NET.Extensions;
using System.Net.Mime;
using System.Text.Json;

namespace Servel.NET;

public class HomeMiddleware(RequestDelegate next, IEnumerable<Root> roots) : MiddlewareBase(next)
{
    public override bool ShouldRun() => Request.IsGetOrHead() && Request.IsRoot();

    public override IResult? Run()
    {
        Response.Headers.Vary = HeaderNames.Accept;

        if(Request.Action() == "list")
        {
            return Results.Text(RenderResponse(), MediaTypeNames.Application.Json);
        }

        return Results.Text(Resources.Get("Views", "home.html"), MediaTypeNames.Text.Html);
    }

    private byte[] RenderResponse()
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            roots.Select(l => new RootEntry(l.UrlPath, l.Name)),
            SerializationSourceGenerationContext.Default.IEnumerableRootEntry);
    }
}
