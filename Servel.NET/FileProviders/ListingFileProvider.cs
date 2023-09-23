using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using System.Reflection;

namespace Servel.NET.FileProviders;

public class ListingFileProvider : PhysicalFileProvider
{
    private static FieldInfo pdcDirectoryField = typeof(PhysicalDirectoryContents).GetField(
        "_directory",
        BindingFlags.NonPublic | BindingFlags.Instance)!;

    public ListingFileProvider(string root) : base(root)
    {
    }

    public ListingFileProvider(string root, ExclusionFilters filters) : base(root, filters)
    {
    }

    public IFileInfo GetDirectoryInfo(string subpath)
    {
        var contents = GetDirectoryContents(subpath);
        if (!contents.Exists) return new NotFoundDirectoryInfo(subpath);

        var pdContents = (PhysicalDirectoryContents)contents;

        var fullPath = (string)pdcDirectoryField.GetValue(pdContents)!;
        return new PhysicalDirectoryInfo(new DirectoryInfo(fullPath));
    }

}
