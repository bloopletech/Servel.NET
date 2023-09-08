using Microsoft.Extensions.FileProviders;

namespace Servel.NET
{
    public class IndexModel
    {
        public required Listing Listing { get; init; }
        public required string Path { get; init; }
        public required IDirectoryContents Contents { get; init; }
    }
}
