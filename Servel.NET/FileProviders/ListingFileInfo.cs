using System.Runtime.CompilerServices;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Servel.NET.FileProviders;

// Based on https://github.com/dotnet/runtime/blob/c8acea22626efab11c13778c028975acdc34678f/src/libraries/Microsoft.Extensions.FileProviders.Physical/src/PhysicalFileInfo.cs
public class ListingFileInfo : IFileInfo
{
    private readonly PhysicalFileInfo _physicalFileInfo;
    private readonly FileInfo _info;
    private readonly FileInfo _resolvedInfo;

    public ListingFileInfo(PhysicalFileInfo physicalFileInfo)
    {
        _physicalFileInfo = physicalFileInfo;
        _info = GetInfoField(_physicalFileInfo);

        try
        {
            var targetInfo = _info.ResolveLinkTarget(true) as FileInfo;
            _resolvedInfo = targetInfo ?? _info;
        }
        catch(FileNotFoundException)
        {
            _resolvedInfo = _info;
        }
    }

    public bool Exists => _resolvedInfo.Exists;

    public long Length => _resolvedInfo.Length;

    public string PhysicalPath => _info.FullName;

    public string Name => _info.Name;

    public DateTimeOffset LastModified => _resolvedInfo.LastWriteTimeUtc;

    public bool IsDirectory => false;

    public Stream CreateReadStream() => _physicalFileInfo.CreateReadStream();

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_info")]
    private extern static ref FileInfo GetInfoField(PhysicalFileInfo @this);
}
