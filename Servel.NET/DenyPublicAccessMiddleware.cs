namespace Servel.NET;

public class DenyPublicAccessMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if(remoteIp == null || !remoteIp.IsPrivate())
        {
            await Results.Forbid().ExecuteAsync(httpContext);
            return;
        }

        await next.Invoke(httpContext);
    }
}