using System.Web;
using Microsoft.Extensions.Caching.Memory;
using Servel.NET.Extensions;
using Servel.NET.FileProviders;

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
        out ListingDirectoryInfo directoryInfo)
    {
        directoryInfo = _listing.FileProvider.GetRequiredDirectoryInfo(requestPath.Value!);
        return ForDirectory(directoryInfo, requestPath, options);
    }

    private DirectoryEntry ForDirectory(
        ListingDirectoryInfo directoryInfo,
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
        ListingDirectoryInfo directoryInfo,
        PathString requestPath,
        ForDirectoryOptions options)
    {
        IEnumerable<OtherEntry>? others = null;
        IEnumerable<DirectoryEntry>? directories = null;
        IEnumerable<FileEntry>? files = null;

        if(options.Depth > 0)
        {
            others = BuildOtherEntries(requestPath);
            directories = directoryInfo.Directories.Select(
                ldi => ForDirectory(ldi, requestPath.Combine(ldi.Name), options.Descend()));
            files = directoryInfo.Files.Select(ForFile);
        }

        var name = requestPath.IsRoot() ? "" : directoryInfo.Name;
        return new DirectoryEntry
        {
            Name = name,
            Url = HttpUtility.UrlPathEncode(name + "/"),
            Mtime = directoryInfo.LastModified.ToUnixTimeMilliseconds(),
            Others = others,
            Directories = directories,
            Files = files,
            Children = options.CountChildren ? directoryInfo.ChildrenCount() : null
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

    private FileEntry ForFile(ListingFileInfo fileInfo) => new FileEntry
    {
        Name = fileInfo.Name,
        Url = HttpUtility.UrlPathEncode(fileInfo.Name),
        Size = fileInfo.Length,
        Mtime = fileInfo.LastModified.ToUnixTimeMilliseconds()
    };
}
