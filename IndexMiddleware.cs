﻿using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Servel.NET
{
    // Derived from https://github.com/dotnet/aspnetcore/blob/5a4c82ec57fadddef9ce841d608de5c7c8c74446/src/Middleware/StaticFiles/src/DirectoryBrowserMiddleware.cs
    public class IndexMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Listing _listing;
        private readonly EntryFactory _entryFactory;

        public IndexMiddleware(RequestDelegate next, Listing listing)
        {
            _next = next;
            _listing = listing;
            _entryFactory = new EntryFactory(_listing);
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (ShouldProcess(httpContext)) await Process(httpContext);
            else await _next.Invoke(httpContext);
        }

        private bool ShouldProcess(HttpContext httpContext) => FileHelpers.IsGetOrHeadMethod(httpContext.Request.Method);

        private async Task Process(HttpContext httpContext)
        {
            // If the path matches a directory but does not end in a slash, redirect to add the slash.
            // This prevents relative links from breaking.
            if (!FileHelpers.PathEndsInSlash(httpContext.Request.Path))
            {
                FileHelpers.RedirectToPathWithSlash(httpContext);
                return;
            }

            var directoryEntryJson = RenderDirectoryEntry(httpContext.Request.Path, ParseParameters(httpContext));

            if(directoryEntryJson == null)
            {
                await Results.NotFound().ExecuteAsync(httpContext);
                return;
            }

            if(httpContext.Request.Headers.Accept.Contains(MediaTypeNames.Application.Json))
            {
                await Results.Content(directoryEntryJson, MediaTypeNames.Application.Json).ExecuteAsync(httpContext);
                return;
            }

            var model = new
            {
                Listing = _listing,
                FullPath = httpContext.Request.PathBase.Add(httpContext.Request.Path).Value!,
                DirectoryEntry = directoryEntryJson
            };

            await Results.Extensions.View("Index", model).ExecuteAsync(httpContext);
        }

        private EntryFactory.ForDirectoryOptions ParseParameters(HttpContext httpContext)
        {
            var depthStr = httpContext.Request.Query["depth"];
            uint.TryParse(depthStr.ToString(), out var depth);
            if(depth == 0) depth = 1;

            var countChildrenStr = httpContext.Request.Query["countChildren"];
            bool.TryParse(countChildrenStr, out var countChildren);

            return new EntryFactory.ForDirectoryOptions
            {
                Depth = depth,
                CountChildren = countChildren
            };
        }

        private string? RenderDirectoryEntry(PathString path, EntryFactory.ForDirectoryOptions options)
        {
            var directoryEntry = _entryFactory.ForDirectory(path, options);
            if (directoryEntry == null) return null;

            JsonSerializerOptions serializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };

            return JsonSerializer.Serialize(directoryEntry, serializerOptions);
        }
    }
}