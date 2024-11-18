using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;

namespace Servel.NET.Extensions;

public static class HttpContextExtensions
{
    public static IDictionary<object, object?> ConnectionItems(this HttpContext context) =>
        context.Features.GetRequiredFeature<IConnectionItemsFeature>().Items;

    public static int SiteId(this HttpContext context) => (int)context.ConnectionItems()["siteId"]!;

    // Derived from https://github.com/dotnet/aspnetcore/blob/5a4c82ec57fadddef9ce841d608de5c7c8c74446/src/Middleware/StaticFiles/src/Helpers.cs
    public static void RedirectToPathWithSlash(this HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
        var request = context.Request;
        context.Response.Headers.Location = UriHelper.BuildAbsolute(
            request.Scheme,
            request.Host,
            request.PathBase,
            request.Path + "/",
            request.QueryString);
    }
}
