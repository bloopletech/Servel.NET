using System.Text.Json.Serialization;
using System.Web;

namespace Servel.NET;

public readonly record struct DirectoryEntry(
    string Name,
    long Mtime,
    IEnumerable<DirectoryEntry>? Directories,
    IEnumerable<FileEntry>? Files,
    IEnumerable<OtherEntry>? Others,
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

public readonly record struct OtherEntry(
    string Name,
    string Href,
    bool HomeEntry,
    bool TopEntry,
    bool ParentEntry);

public readonly record struct ListingEntry(string Href, [property: JsonIgnore()] string? CustomName)
{
    public string Name => CustomName ?? HttpUtility.UrlDecode(Href[1..]);
}