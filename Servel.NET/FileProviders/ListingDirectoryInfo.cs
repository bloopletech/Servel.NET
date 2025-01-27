using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileProviders;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Servel.NET.FileProviders;

// Based on https://github.com/dotnet/runtime/blob/c8acea22626efab11c13778c028975acdc34678f/src/libraries/Microsoft.Extensions.FileProviders.Physical/src/PhysicalDirectoryInfo.cs
public class ListingDirectoryInfo : IFileInfo, IDirectoryContents
{
    private readonly PhysicalDirectoryInfo _physicalDirectoryInfo;
    private readonly DirectoryInfo _info;
    private readonly DirectoryInfo _resolvedInfo;

    public ListingDirectoryInfo(PhysicalDirectoryInfo physicalDirectoryInfo)
    {
        _physicalDirectoryInfo = physicalDirectoryInfo;
        _info = GetInfoField(_physicalDirectoryInfo);

        try
        {
            var targetInfo = _info.ResolveLinkTarget(true) as DirectoryInfo;
            _resolvedInfo = targetInfo ?? _info;
        }
        catch(IOException)
        {
            _resolvedInfo = _info;
        }
    }

    public bool Exists => _resolvedInfo.Exists;

    public long Length => -1;

    public string PhysicalPath => _info.FullName;

    public string Name => _info.Name;

    public DateTimeOffset LastModified => _resolvedInfo.LastWriteTimeUtc;

    public bool IsDirectory => true;

    public Stream CreateReadStream() => _physicalDirectoryInfo.CreateReadStream();

    public IEnumerator<IFileInfo> GetEnumerator() => EnumerateEntries();

    IEnumerator IEnumerable.GetEnumerator() => EnumerateEntries();

    [SuppressMessage("Performance", "CA1829")]
    [SuppressMessage("CodeQuality", "IDE0079")]
    public int ChildrenCount() => this.Count();

    public IEnumerable<ListingDirectoryInfo> Directories => this.OfType<ListingDirectoryInfo>();
    public IEnumerable<ListingFileInfo> Files => this.OfType<ListingFileInfo>();

    private IEnumerator<IFileInfo> EnumerateEntries()
    {
        foreach(var info in _physicalDirectoryInfo)
        {
            yield return info switch
            {
                PhysicalFileInfo pfi => new ListingFileInfo(pfi),
                PhysicalDirectoryInfo pdi => new ListingDirectoryInfo(pdi),
                _ => throw new InvalidOperationException()
            };
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_info")]
    private extern static ref DirectoryInfo GetInfoField(PhysicalDirectoryInfo @this);
}