using Microsoft.Extensions.FileProviders;
using System.Diagnostics.CodeAnalysis;

namespace Servel.NET.FileProviders;

// Derived from https://github.com/dotnet/runtime/blob/a6dbb800a47735bde43187350fd3aff4071c7f9c/src/libraries/Microsoft.Extensions.FileProviders.Abstractions/src/NotFoundFileInfo.cs
/// <summary>
/// Represents a non-existing directory.
/// </summary>
public class NotFoundDirectoryInfo : IFileInfo
{
    /// <summary>
    /// Initializes an instance of <see cref="NotFoundDirectoryInfo"/>.
    /// </summary>
    /// <param name="name">The name of the directory that could not be found</param>
    public NotFoundDirectoryInfo(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Always false.
    /// </summary>
    public bool Exists => false;

    /// <summary>
    /// Always true.
    /// </summary>
    public bool IsDirectory => true;

    /// <summary>
    /// Returns <see cref="DateTimeOffset.MinValue"/>.
    /// </summary>
    public DateTimeOffset LastModified => DateTimeOffset.MinValue;

    /// <summary>
    /// Always equals -1.
    /// </summary>
    public long Length => -1;

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Always null.
    /// </summary>
    public string? PhysicalPath => null;

    /// <summary>
    /// Always throws. A stream cannot be created for non-existing directory.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Always thrown.</exception>
    /// <returns>Does not return</returns>
    [DoesNotReturn]
    public Stream CreateReadStream()
    {
        throw new DirectoryNotFoundException($"The directory {Name} does not exist.");
    }
}
