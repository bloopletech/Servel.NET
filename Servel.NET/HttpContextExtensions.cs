using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;

namespace Servel.NET;

public static class HttpContextExtensions
{
    public static IDictionary<object, object?> ConnectionItems(this HttpContext context) =>
        context.Features.GetRequiredFeature<IConnectionItemsFeature>().Items;

    public static int SiteId(this HttpContext context) => (int)context.ConnectionItems()["siteId"]!;
}
