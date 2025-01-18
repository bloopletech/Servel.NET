using Microsoft.Extensions.FileProviders;

namespace Servel.NET.Extensions;

public static class IFileProviderExtensions
{
    public static IFileInfo GetRequiredFileInfo(this IFileProvider fileProvider, string subpath)
    {
        var fileInfo = fileProvider.GetFileInfo(subpath);
        if(!fileInfo.Exists) throw new FileNotFoundException(subpath);
        return fileInfo;
    }
}
