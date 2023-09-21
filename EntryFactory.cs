using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders.Physical;
using System.Web;

namespace Servel.NET
{
    public class EntryFactory
    {
        public readonly record struct ForDirectoryOptions(uint Depth, bool CountChildren)
        {
            public ForDirectoryOptions Descend()
            {
                return this with { Depth = Depth - 1 };
            }
        }

        private static readonly SpecialEntry HomeEntry = new SpecialEntry
        {
            HomeEntry = true,
            Name = "Listings Home",
            Href = "/"
        };

        private static readonly SpecialEntry ParentEntry = new SpecialEntry
        {
            ParentEntry = true,
            Name = "Parent Directory",
            Href = "../"
        };

        private readonly Listing _listing;
        private readonly SpecialEntry _topEntry;
        private readonly IMemoryCache _memoryCache;

        public EntryFactory(Listing listing, IMemoryCache memoryCache)
        {
            _listing = listing;
            _topEntry = new SpecialEntry
            {
                TopEntry = true,
                Name = "Top Directory",
                Href = HttpUtility.UrlPathEncode(_listing.RequestPath)
            };
            _memoryCache = memoryCache;
        }

        public DirectoryEntry? ForDirectory(PathString requestPath, ForDirectoryOptions options)
        {
            var directoryInfo = _listing.FileProvider.GetDirectoryInfo(requestPath.Value!);
            if (!directoryInfo.Exists) return null;

            return ForDirectory((PhysicalDirectoryInfo)directoryInfo, requestPath, options);
        }

        private DirectoryEntry ForDirectory(
            PhysicalDirectoryInfo directoryInfo,
            PathString requestPath,
            ForDirectoryOptions options)
        {
            var cacheKey = (directoryInfo.PhysicalPath, directoryInfo.LastModified, options);
            return _memoryCache.GetOrCreate(cacheKey, (cacheEntry) =>
            {
                return ForDirectoryWithoutCache(directoryInfo, requestPath, options);
            })!;
        }

        private DirectoryEntry ForDirectoryWithoutCache(
            PhysicalDirectoryInfo directoryInfo,
            PathString requestPath,
            ForDirectoryOptions options)
        {
            IEnumerable<SpecialEntry>? specialEntries = null;
            IEnumerable<DirectoryEntry>? directoryEntries = null;
            IEnumerable<FileEntry>? fileEntries = null;
            int? childCount = null;

            if(options.Depth > 0)
            {
                var contents = _listing.FileProvider.GetDirectoryContents(requestPath.Value!);
                if (contents.Exists)
                {
                    specialEntries = BuildSpecialEntries(requestPath);
                    directoryEntries = contents.OfType<PhysicalDirectoryInfo>()
                        .Select(pdi => ForDirectory(pdi, requestPath.Combine(pdi.Name), options.Descend()));
                    fileEntries = contents.OfType<PhysicalFileInfo>().Select(ForFile);
                    if(options.CountChildren) childCount = directoryEntries.Count() + fileEntries.Count();
                }
            }
            else if(options.CountChildren)
            {
                var contents = _listing.FileProvider.GetDirectoryContents(requestPath.Value!);
                childCount = contents.Count();
            }

            return new DirectoryEntry
            {
                Name = requestPath.IsRoot() ? "" : directoryInfo.Name,
                Mtime = directoryInfo.LastModified.ToUnixTimeMilliseconds(),
                SpecialEntries = specialEntries,
                Directories = directoryEntries,
                Files = fileEntries,
                Children = childCount
            };
        }

        private IEnumerable<SpecialEntry> BuildSpecialEntries(PathString requestPath)
        {
            var list = new List<SpecialEntry>();
            if (!_listing.IsMountAtRoot) list.Add(HomeEntry);
            if (!requestPath.IsRoot())
            {
                list.Add(_topEntry);
                list.Add(ParentEntry);
            }

            return list;
        }

        private FileEntry ForFile(PhysicalFileInfo fileInfo)
        {
            return new FileEntry
            {
                Name = fileInfo.Name,
                Size = fileInfo.Length,
                Mtime = fileInfo.LastModified.ToUnixTimeMilliseconds()
            };
        }
    }
}
