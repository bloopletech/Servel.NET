using System.Net;
using Servel.NET.Extensions;

namespace Servel.NET;

public class DenyAudienceMiddleware(RequestDelegate next, Audience audience) : MiddlewareBase(next)
{
    public override IResult? Before()
    {
        var remoteIp = Connection.RemoteIpAddress;

        var allow = audience switch
        {
            Audience.Localhost => remoteIp != null && IPAddress.IsLoopback(remoteIp),
            Audience.LocalNetwork => remoteIp != null && remoteIp.IsPrivate(),
            Audience.Public => true,
            _ => throw new NotImplementedException()
        };

        return allow ? null : Results.Forbid();
    }
}