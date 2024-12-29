using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using System.Runtime.CompilerServices;

namespace Servel.NET.FileProviders;

public class ListingFileProvider : PhysicalFileProvider
{
    public ListingFileProvider(string root) : base(root)
    {
    }

    public ListingFileProvider(string root, ExclusionFilters filters) : base(root, filters)
    {
    }

    public IFileInfo GetDirectoryInfo(string subpath)
    {
        var contents = GetDirectoryContents(subpath);
        if(!contents.Exists) return new NotFoundDirectoryInfo(subpath);

        return GetInfoField((PhysicalDirectoryContents)contents);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_info")]
    private extern static ref PhysicalDirectoryInfo GetInfoField(PhysicalDirectoryContents @this);
}
