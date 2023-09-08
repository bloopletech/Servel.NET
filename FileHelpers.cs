using Microsoft.AspNetCore.Http.Extensions;

namespace Servel.NET
{
    // Derived from https://github.com/dotnet/aspnetcore/blob/5a4c82ec57fadddef9ce841d608de5c7c8c74446/src/Middleware/StaticFiles/src/Helpers.cs
    public static class FileHelpers
    {
        public static bool IsGetOrHeadMethod(string method)
        {
            return HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
        }

        public static bool PathEndsInSlash(PathString path)
        {
            return path.HasValue && path.Value!.EndsWith("/", StringComparison.Ordinal);
        }

        public static string GetPathValueWithSlash(PathString path)
        {
            if (!PathEndsInSlash(path))
            {
                return path.Value + "/";
            }
            return path.Value!;
        }

        public static void RedirectToPathWithSlash(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
            var request = context.Request;
            var redirect = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path + "/", request.QueryString);
            context.Response.Headers.Location = redirect;
        }

        public static bool TryMatchPath(HttpContext context, PathString matchUrl, bool forDirectory, out PathString subpath)
        {
            var path = context.Request.Path;

            if (forDirectory && !PathEndsInSlash(path))
            {
                path += new PathString("/");
            }

            if (path.StartsWithSegments(matchUrl, out subpath))
            {
                return true;
            }
            return false;
        }
    }
}
