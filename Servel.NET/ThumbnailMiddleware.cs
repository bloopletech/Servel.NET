using Servel.NET.Extensions;
using Servel.NET.Services;

namespace Servel.NET;

public class ThumbnailMiddleware(
    RequestDelegate next,
    Listing listing,
    DirectoryOptionsResolver directoryOptionsResolver,
    ThumbnailService thumbnailsService) : MiddlewareBase(next)
{
    public override bool ShouldRun() => Request.IsGetOrHead() && Request.Action() == "thumbnail";

    public override async Task<IResult?> RunAsync()
    {
        var requestPath = Request.Path;
        var fileInfo = listing.FileProvider.GetRequiredFileInfo(requestPath.Value!);

        //is it a media file???????
        var data = await thumbnailsService.FindOrCreateByPath(fileInfo) ?? throw new FileNotFoundException(requestPath);

        //TODO send the most efficient way
        await Response.BodyWriter.WriteAsync(data);
        //Response.Headers.ContentDisposition = "inline";
        //await Results.File(data).ExecuteAsync(HttpContext);
        return Results.Empty;
    }
}
