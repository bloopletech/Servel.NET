using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using System.Runtime.CompilerServices;

namespace Servel.NET.FileProviders;

public class ListingFileProvider : IFileProvider, IDisposable
{
    private PhysicalFileProvider physicalProvider;

    public ListingFileProvider(string root)
    {
        physicalProvider = new PhysicalFileProvider(root);
    }

    public ListingFileProvider(string root, ExclusionFilters filters)
    {
        physicalProvider = new PhysicalFileProvider(root, filters);
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var originalContents = physicalProvider.GetFileInfo(subpath);
        if(originalContents is NotFoundFileInfo) return originalContents;

        return new ListingFileInfo((PhysicalFileInfo)originalContents);
    }

    public ListingFileInfo GetRequiredFileInfo(string subpath)
    {
        var fileInfo = GetFileInfo(subpath);
        if(!fileInfo.Exists) throw new FileNotFoundException(subpath);
        return (ListingFileInfo)fileInfo;
    }

    public IDirectoryContents GetDirectoryContents(string subpath) => physicalProvider.GetDirectoryContents(subpath);

    public IFileInfo GetDirectoryInfo(string subpath)
    {
        var contents = GetDirectoryContents(subpath);
        if(!contents.Exists) return new NotFoundDirectoryInfo(subpath);

        var info = GetInfoField((PhysicalDirectoryContents)contents);
        return new ListingDirectoryInfo(info);
    }

    public ListingDirectoryInfo GetRequiredDirectoryInfo(string subpath)
    {
        var directoryInfo = GetDirectoryInfo(subpath);
        if(!directoryInfo.Exists) throw new DirectoryNotFoundException(subpath);
        return (ListingDirectoryInfo)directoryInfo;
    }

    public IChangeToken Watch(string filter) => physicalProvider.Watch(filter);

    public void Dispose() => physicalProvider.Dispose();

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_info")]
    private extern static ref PhysicalDirectoryInfo GetInfoField(PhysicalDirectoryContents @this);
}
