using System.Runtime.CompilerServices;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Servel.NET.FileProviders;

// Based on https://github.com/dotnet/runtime/blob/c8acea22626efab11c13778c028975acdc34678f/src/libraries/Microsoft.Extensions.FileProviders.Physical/src/PhysicalFileInfo.cs
public class LinkAwareFileInfo : IFileInfo
{
    private readonly PhysicalFileInfo wrapped;
    private readonly FileInfo info;
    private readonly FileInfo resolvedInfo;

    public LinkAwareFileInfo(PhysicalFileInfo physicalFileInfo)
    {
        wrapped = physicalFileInfo;
        info = GetInfoField(wrapped);
        var targetInfo = info.ResolveLinkTarget(true) as FileInfo;
        resolvedInfo = targetInfo ?? info;
    }

    public bool Exists => resolvedInfo.Exists;

    public long Length => resolvedInfo.Length;

    public string PhysicalPath => info.FullName;

    public string Name => info.Name;

    public DateTimeOffset LastModified => resolvedInfo.LastWriteTimeUtc;

    public bool IsDirectory => false;

    public Stream CreateReadStream() => wrapped.CreateReadStream();

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_info")]
    private extern static ref FileInfo GetInfoField(PhysicalFileInfo @this);
}
