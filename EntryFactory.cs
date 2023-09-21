using Microsoft.Extensions.FileProviders.Physical;
using System.Web;

namespace Servel.NET
{
    public class EntryFactory
    {
        public class ForDirectoryOptions
        {
            public uint Depth { get; init; }
            public bool CountChildren { get; init; }
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

        public EntryFactory(Listing listing)
        {
            _listing = listing;
            _topEntry = new SpecialEntry
            {
                TopEntry = true,
                Name = "Top Directory",
                Href = HttpUtility.UrlPathEncode(_listing.RequestPath)
            };
        }

        public DirectoryEntry? ForDirectory(PathString requestPath, ForDirectoryOptions options)
        {
            var directoryInfo = _listing.FileProvider.GetDirectoryInfo(requestPath.Value!);
            if (!directoryInfo.Exists) return null;

            return ForDirectory((PhysicalDirectoryInfo)directoryInfo, requestPath, options, options.Depth);
        }

        private DirectoryEntry ForDirectory(
            PhysicalDirectoryInfo directoryInfo,
            PathString requestPath,
            ForDirectoryOptions options,
            uint depth)
        {
            IEnumerable<SpecialEntry>? specialEntries = null;
            IEnumerable<DirectoryEntry>? directoryEntries = null;
            IEnumerable<FileEntry>? fileEntries = null;
            int? childCount = null;

            if(depth > 0)
            {
                var contents = _listing.FileProvider.GetDirectoryContents(requestPath.Value!);
                if (contents.Exists)
                {
                    specialEntries = BuildSpecialEntries(requestPath);
                    directoryEntries = contents.OfType<PhysicalDirectoryInfo>()
                        .Select(pdi => ForDirectory(pdi, requestPath + pdi.Name, options, depth - 1));
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
