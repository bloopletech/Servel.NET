using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileProviders;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Servel.NET.FileProviders;

// Based on https://github.com/dotnet/runtime/blob/c8acea22626efab11c13778c028975acdc34678f/src/libraries/Microsoft.Extensions.FileProviders.Physical/src/PhysicalDirectoryInfo.cs
public class LinkAwareDirectoryInfo : IFileInfo, IDirectoryContents
{
    private readonly PhysicalDirectoryInfo wrapped;
    private readonly DirectoryInfo info;
    private readonly DirectoryInfo resolvedInfo;

    public LinkAwareDirectoryInfo(PhysicalDirectoryInfo physicalDirectoryInfo)
    {
        wrapped = physicalDirectoryInfo;
        info = GetInfoField(wrapped);
        var targetInfo = info.ResolveLinkTarget(true) as DirectoryInfo;
        resolvedInfo = targetInfo ?? info;
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

    private IEnumerator<IFileInfo> EnumerateEntries()
    {
        foreach(var info in wrapped)
        {
            yield return info switch
            {
                PhysicalFileInfo pfi => new LinkAwareFileInfo(pfi),
                PhysicalDirectoryInfo pdi => new LinkAwareDirectoryInfo(pdi),
                _ => throw new InvalidOperationException()
            };
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_info")]
    private extern static ref DirectoryInfo GetInfoField(PhysicalDirectoryInfo @this);
}