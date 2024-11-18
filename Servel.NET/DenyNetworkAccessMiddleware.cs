using System.Net;

namespace Servel.NET;

public class DenyNetworkAccessMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (remoteIp == null || !IPAddress.IsLoopback(remoteIp))
        {
            await Results.StatusCode(StatusCodes.Status403Forbidden).ExecuteAsync(httpContext);
            return;
        }

        await next.Invoke(httpContext);
    }
}