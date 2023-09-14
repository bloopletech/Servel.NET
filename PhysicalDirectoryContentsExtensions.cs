using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using System.Reflection;

namespace Servel.NET
{
    public static class PhysicalDirectoryContentsExtensions
    {
        private static FieldInfo directoryField = typeof(PhysicalDirectoryContents).GetField(
            "_directory",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        public static PhysicalDirectoryInfo GetDirectoryInfo(this PhysicalDirectoryContents contents)
        {
            var directory = (string)directoryField.GetValue(contents)!;
            return new PhysicalDirectoryInfo(new DirectoryInfo(directory));
        }
    }
}
