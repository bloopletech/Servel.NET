using Servel.NET.Extensions;
using Servel.NET.Services;

namespace Servel.NET;

public class JwtAuthenticationMiddleware(RequestDelegate next, Site site) : MiddlewareBase(next)
{
    private readonly JwtService jwtService = new(site);

    private string? Token => Request.Param("token");

    public override bool ShouldRun() => HttpContext.IsUnauthenticated() && Token != null;

    public override IResult? Before()
    {
        if(jwtService.Validate(Token!, Request.Path.Value!))
        {
            HttpContext.User = BasicAuthenticationMiddleware.User;
            return null;
        }

        return Results.Unauthorized();
    }
}