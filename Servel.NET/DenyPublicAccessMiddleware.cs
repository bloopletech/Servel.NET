using Servel.NET.Extensions;

namespace Servel.NET;

public class DenyPublicAccessMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if(remoteIp == null || !remoteIp.IsPrivate())
        {
            await Results.StatusCode(StatusCodes.Status403Forbidden).ExecuteAsync(httpContext);
            return;
        }

        await next.Invoke(httpContext);
    }
}