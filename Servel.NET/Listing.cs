using Servel.NET.FileProviders;

namespace Servel.NET;

public class Listing(string rootPath, string requestPath)
{
    public string RootPath = rootPath;
    public string RequestPath = requestPath;
    public ListingFileProvider FileProvider = new(rootPath);

    public bool IsMountAtRoot => RequestPath == "/";
}
