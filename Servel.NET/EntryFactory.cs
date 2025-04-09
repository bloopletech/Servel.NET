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
        Name = "Home",
        Url = "/"
    };

    private static readonly OtherEntry ParentEntry = new()
    {
        ParentEntry = true,
        Name = "Parent Directory",
        Url = "../"
    };

    private readonly Root _root;
    private readonly OtherEntry _topEntry;

    public EntryFactory(Root root)
    {
        _root = root;
        _topEntry = new OtherEntry
        {
            TopEntry = true,
            Name = "Top Directory",
            Url = _root.UrlPath
        };
    }

    public DirectoryEntry ForDirectory(
        PathString requestPath,
        ForDirectoryOptions options,
        out LinkAwareDirectoryInfo directoryInfo)
    {
        directoryInfo = _root.FileProvider.GetRequiredDirectoryInfo(requestPath.Value!);
        return ForDirectory(directoryInfo, requestPath, options);
    }

    private DirectoryEntry ForDirectory(
        LinkAwareDirectoryInfo directoryInfo,
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
            Url = UrlUtility.EncodeUrlPath(name + "/"),
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
        if(!_root.IsMountAtRoot) list.Add(HomeEntry);
        if(!requestPath.IsRoot())
        {
            list.Add(_topEntry);
            list.Add(ParentEntry);
        }

        return list;
    }

    private FileEntry ForFile(LinkAwareFileInfo fileInfo) => new()
    {
        Name = fileInfo.Name,
        Url = UrlUtility.EncodeUrlPath(fileInfo.Name),
        Size = fileInfo.Length,
        Mtime = fileInfo.LastModified.ToUnixTimeMilliseconds()
    };
}
