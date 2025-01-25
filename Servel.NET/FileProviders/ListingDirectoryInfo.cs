using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileProviders;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Servel.NET.FileProviders;

// Based on https://github.com/dotnet/runtime/blob/c8acea22626efab11c13778c028975acdc34678f/src/libraries/Microsoft.Extensions.FileProviders.Physical/src/PhysicalDirectoryInfo.cs
public class ListingDirectoryInfo : IFileInfo, IDirectoryContents
{
    private readonly PhysicalDirectoryInfo wrapped;
    private readonly DirectoryInfo info;
    private readonly DirectoryInfo resolvedInfo;

    public ListingDirectoryInfo(PhysicalDirectoryInfo physicalDirectoryInfo)
    {
        wrapped = physicalDirectoryInfo;
        info = GetInfoField(wrapped);

        try
        {
            var targetInfo = info.ResolveLinkTarget(true) as DirectoryInfo;
            resolvedInfo = targetInfo ?? info;
        }
        catch(DirectoryNotFoundException)
        {
            resolvedInfo = info;
        }
    }

    public bool Exists => resolvedInfo.Exists;

    public long Length => -1;

    public string PhysicalPath => info.FullName;

    public string Name => info.Name;

    public DateTimeOffset LastModified => resolvedInfo.LastWriteTimeUtc;

    public bool IsDirectory => true;

    public Stream CreateReadStream() => wrapped.CreateReadStream();

    public IEnumerator<IFileInfo> GetEnumerator() => EnumerateEntries();

    IEnumerator IEnumerable.GetEnumerator() => EnumerateEntries();

    [SuppressMessage("Performance", "CA1829")]
    [SuppressMessage("CodeQuality", "IDE0079")]
    public int ChildrenCount() => this.Count();

    public IEnumerable<ListingDirectoryInfo> Directories => this.OfType<ListingDirectoryInfo>();
    public IEnumerable<ListingFileInfo> Files => this.OfType<ListingFileInfo>();

    private IEnumerator<IFileInfo> EnumerateEntries()
    {
        foreach(var info in wrapped)
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