using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders.Physical;
using Servel.NET.Extensions;

namespace Servel.NET;

public class EntryFactory
{
    public readonly record struct ForDirectoryOptions(uint Depth, bool CountChildren)
    {
        public ForDirectoryOptions Descend()
        {
            return this with { Depth = Depth - 1 };
        }
    }

    private static readonly OtherEntry HomeEntry = new()
    {
        HomeEntry = true,
        Name = "Listings Home",
        Url = "/"
    };

    private static readonly OtherEntry ParentEntry = new()
    {
        ParentEntry = true,
        Name = "Parent Directory",
        Url = "../"
    };

    private readonly Listing _listing;
    private readonly OtherEntry _topEntry;
    private readonly IMemoryCache _memoryCache;

    public EntryFactory(Listing listing, IMemoryCache memoryCache)
    {
        _listing = listing;
        _topEntry = new OtherEntry
        {
            TopEntry = true,
            Name = "Top Directory",
            Url = _listing.UrlPath
        };
        _memoryCache = memoryCache;
    }

    public DirectoryEntry ForDirectory(
        PathString requestPath,
        ForDirectoryOptions options,
        out PhysicalDirectoryInfo directoryInfo)
    {
        directoryInfo = _listing.FileProvider.GetRequiredDirectoryInfo(requestPath.Value!);
        return ForDirectory(directoryInfo, requestPath, options);
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
        IEnumerable<OtherEntry>? otherEntries = null;
        IEnumerable<DirectoryEntry>? directoryEntries = null;
        IEnumerable<FileEntry>? fileEntries = null;
        int? childCount = null;

        if(options.Depth > 0)
        {
            otherEntries = BuildOtherEntries(requestPath);
            directoryEntries = directoryInfo.OfType<PhysicalDirectoryInfo>()
                .Select(pdi => ForDirectory(pdi, requestPath.Combine(pdi.Name), options.Descend()));
            fileEntries = directoryInfo.OfType<PhysicalFileInfo>().Select(ForFile);
            if(options.CountChildren) childCount = directoryEntries.Count() + fileEntries.Count();
        }
        else if(options.CountChildren)
        {
#pragma warning disable CA1829
            childCount = directoryInfo.Count();
#pragma warning restore CA1829
        }

        return new DirectoryEntry
        {
            Name = requestPath.IsRoot() ? "" : directoryInfo.Name,
            Mtime = directoryInfo.LastModified.ToUnixTimeMilliseconds(),
            Others = otherEntries,
            Directories = directoryEntries,
            Files = fileEntries,
            Children = childCount
        };
    }

    private List<OtherEntry> BuildOtherEntries(PathString requestPath)
    {
        var list = new List<OtherEntry>();
        if(!_listing.IsMountAtRoot) list.Add(HomeEntry);
        if(!requestPath.IsRoot())
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
