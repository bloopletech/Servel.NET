using Servel.NET.FileProviders;

namespace Servel.NET;

public class Listing
{
    public string RootPath;
    public string RequestPath;
    public ListingFileProvider FileProvider;

    public Listing(string rootPath, string requestPath)
    {
        RootPath = rootPath;
        RequestPath = requestPath;
        FileProvider = new ListingFileProvider(rootPath);
    }

    public bool IsMountAtRoot => RequestPath == "/";
}
