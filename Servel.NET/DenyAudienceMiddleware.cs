using System.Net;
using Servel.NET.Extensions;

namespace Servel.NET;

public class DenyAudienceMiddleware(RequestDelegate next, Audience audience)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;

        var allow = audience switch
        {
            Audience.Localhost => remoteIp != null && IPAddress.IsLoopback(remoteIp),
            Audience.LocalNetwork => remoteIp != null && remoteIp.IsPrivate(),
            Audience.Public => true,
            _ => throw new NotImplementedException()
        };

        if (!allow) await Results.StatusCode(StatusCodes.Status403Forbidden).ExecuteAsync(httpContext);
        else await next.Invoke(httpContext);
    }
}