using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using System.Text.Json;

namespace Servel.NET
{
    // Derived from https://github.com/dotnet/aspnetcore/blob/5a4c82ec57fadddef9ce841d608de5c7c8c74446/src/Middleware/StaticFiles/src/DirectoryBrowserMiddleware.cs
    public class IndexMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Listing _listing;

        public IndexMiddleware(RequestDelegate next, Listing listing)
        {
            _next = next;
            _listing = listing;

        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (ShouldProcess(httpContext)) await Process(httpContext);
            else await _next.Invoke(httpContext);
        }

        private bool ShouldProcess(HttpContext httpContext) => FileHelpers.IsGetOrHeadMethod(httpContext.Request.Method)
            && GetDirectoryContents(httpContext).Exists;

        private async Task Process(HttpContext httpContext)
        {
            // If the path matches a directory but does not end in a slash, redirect to add the slash.
            // This prevents relative links from breaking.
            if (!FileHelpers.PathEndsInSlash(httpContext.Request.Path))
            {
                FileHelpers.RedirectToPathWithSlash(httpContext);
                return;
            }

            var contents = GetDirectoryContents(httpContext);

            IEnumerable<Entry> directories = contents.OfType<PhysicalDirectoryInfo>().Select(EntryFactory.For).Where(x => x != null).Select(e => e!);
            IEnumerable<Entry> files = contents.OfType<PhysicalFileInfo>().Select(EntryFactory.For).Where(x => x != null).Select(e => e!);

            //List<Entry> children = contents.Select(EntryFactory.For).Where(x => x != null).Select(e => e!).ToList();

            var model = new
            {
                Listing = _listing,
                FullPath = httpContext.Request.PathBase.Add(httpContext.Request.Path).Value!,
                SpecialEntries = JsonSerializer.Serialize(SpecialEntries(httpContext)),
                DirectoryEntries = JsonSerializer.Serialize(directories),
                FileEntries = JsonSerializer.Serialize(files)
            };

            await Results.Extensions.View("Index", model).ExecuteAsync(httpContext);
        }

        private List<Entry> SpecialEntries(HttpContext httpContext)
        {
            var list = new List<Entry>();
            if (_listing.RequestPath != "/") list.Add(EntryFactory.HomeEntry);
            if(httpContext.Request.Path != "/")
            {
                list.Add(EntryFactory.Top(httpContext.Request.PathBase + "/"));
                list.Add(EntryFactory.ParentEntry);
            }

            return list;
        }

        private IDirectoryContents GetDirectoryContents(HttpContext httpContext)
        {
            return _listing.FileProvider.GetDirectoryContents(httpContext.Request.Path.Value!);
        }
    }
}
