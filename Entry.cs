using System.Web;

namespace Servel.NET
{
    public readonly record struct DirectoryEntry(
        string Name,
        long Mtime,
        bool HomeEntry,
        bool TopEntry,
        bool ParentEntry,
        IEnumerable<DirectoryEntry>? Directories,
        IEnumerable<FileEntry>? Files,
        IEnumerable<SpecialEntry>? SpecialEntries,
        int? Children)
    { 
        public string Href => HttpUtility.UrlPathEncode(Name + "/");
    }

    public readonly record struct FileEntry(
        string Name,
        long Size,
        long Mtime,
        bool Video,
        bool Image,
        bool Audio,
        bool Text)
    { 
        public string Href => HttpUtility.UrlPathEncode(Name);
    }

    public readonly record struct SpecialEntry(
        string Name,
        string Href,
        bool HomeEntry,
        bool TopEntry,
        bool ParentEntry);
}
