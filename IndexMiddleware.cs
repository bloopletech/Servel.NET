using Microsoft.Extensions.FileProviders;
using System.Text.Json;

namespace Servel.NET
{
    public class IndexMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Listing _listing;

        public IndexMiddleware(RequestDelegate next, Listing listing)
        {
            _next = next;
            _listing = listing;

        }

        public async Task Invoke(HttpContext httpContext)
        {
            if(FileHelpers.IsGetOrHeadMethod(httpContext.Request.Method)
                && FileHelpers.TryMatchPath(httpContext, _listing.RequestPath, forDirectory: true, subpath: out var subpath)
                && TryGetDirectoryInfo(subpath, out var contents))
            {
                // If the path matches a directory but does not end in a slash, redirect to add the slash.
                // This prevents relative links from breaking.
                if (!FileHelpers.PathEndsInSlash(httpContext.Request.Path))
                {
                    FileHelpers.RedirectToPathWithSlash(httpContext);
                    return;
                }

                List<Entry> children = contents.Select(EntryFactory.For).Where(x => x != null).Select(e => e!).ToList();

                var model = new {
                    Listing = _listing,
                    Path = httpContext.Request.Path,
                    SpecialEntries = JsonSerializer.Serialize(SpecialEntries(subpath).Select(e => e.AsJson())),
                    DirectoryEntries = JsonSerializer.Serialize(children.Where(e => e.IsDirectory()).Select(e => e.AsJson())),
                    FileEntries = JsonSerializer.Serialize(children.Where(e => e.IsFile()).Select(e => e.AsJson()))
                };

                await Results.Extensions.View("Index", model).ExecuteAsync(httpContext);
                return;
            }

            await _next.Invoke(httpContext);
        }

        private bool TryGetDirectoryInfo(PathString subpath, out IDirectoryContents contents)
        {
            // TryMatchPath will not output an empty subpath when it returns true. This is called only in that case.
            contents = _listing.FileProvider.GetDirectoryContents(subpath.Value!);
            return contents.Exists;
        }

        private List<Entry> SpecialEntries(string subpath)
        {
            var list = new List<Entry>();
            if (_listing.RequestPath != "") list.Add(EntryFactory.Home("/"));
            if(subpath != "/")
            {
                list.Add(EntryFactory.Top(_listing.RequestPath == "" ? "/" : _listing.RequestPath));
                list.Add(EntryFactory.Parent("../"));
            }

            return list;
        }
    }
}
