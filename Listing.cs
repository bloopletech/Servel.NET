using Microsoft.Extensions.FileProviders;

namespace Servel.NET
{
    public class Listing
    {
        public string RootPath;
        public string RequestPath;
        public PhysicalFileProvider FileProvider;

        public Listing(string rootPath, string requestPath)
        {
            RootPath = rootPath;
            RequestPath = requestPath;
            FileProvider = new PhysicalFileProvider(rootPath);
        }

        public bool IsMountAtRoot => RequestPath == "/";
    }
}
