using Microsoft.AspNetCore.StaticFiles;

namespace Servel.NET;

public static class ContentTypeProvider
{
    public static IContentTypeProvider Provider { get; private set; }

    static ContentTypeProvider()
    {
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings.Add(".mkv", "video/matroska");
        Provider = provider;
    }
}
