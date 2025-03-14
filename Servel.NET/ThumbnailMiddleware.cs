using Servel.NET.Extensions;
using Servel.NET.Services;

namespace Servel.NET;

public class ThumbnailMiddleware(
    RequestDelegate next,
    Listing listing,
    DirectoryOptionsResolver directoryOptionsResolver,
    ThumbnailService thumbnailsService)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if(ShouldProcess(httpContext)) await Process(httpContext);
        else await next.Invoke(httpContext);
    }

    private static bool ShouldProcess(HttpContext httpContext) =>
        httpContext.Request.IsGetOrHead() && httpContext.Request.GetAction() == "thumbnail";

    private async Task Process(HttpContext httpContext)
    {
        var requestPath = httpContext.Request.Path;
        var fileInfo = listing.FileProvider.GetRequiredFileInfo(requestPath.Value!);

        //is it a media file???????
        var data = await thumbnailsService.FindOrCreateByPath(fileInfo) ?? throw new FileNotFoundException(requestPath);

        //TODO send the most efficient way
        await httpContext.Response.BodyWriter.WriteAsync(data);
        //httpContext.Response.Headers.ContentDisposition = "inline";
        //await Results.File(data).ExecuteAsync(httpContext);
    }
}
