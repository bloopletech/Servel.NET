using Servel.NET.Extensions;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using Servel.NET.Services;

namespace Servel.NET;

public class SignedUrlMiddleware(RequestDelegate next, Site site) : MiddlewareBase(next)
{
    private readonly JwtService jwtService = new(site);

    public override bool ShouldRun() => Request.IsPost() && Request.Action() == "signed-url";

    public override IResult? Run()
    {
        var token = jwtService.Generate(Request.Path.Value!);

        var signedUrl = UriHelper.BuildAbsolute(
            Request.Scheme,
            Request.Host,
            Request.PathBase,
            Request.Path,
            QueryString.Create("token", token));
        return Results.Text(signedUrl, MediaTypeNames.Text.Plain);
    }
}
