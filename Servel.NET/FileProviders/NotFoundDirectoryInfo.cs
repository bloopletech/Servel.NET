using Microsoft.Extensions.FileProviders;
using System.Diagnostics.CodeAnalysis;

namespace Servel.NET.FileProviders;

public class NotFoundDirectoryInfo(string name) : NotFoundFileInfo(name)
{
    public new bool IsDirectory => true;

    [DoesNotReturn]
    public new Stream CreateReadStream()
    {
        throw new DirectoryNotFoundException($"The directory {Name} does not exist.");
    }
}
