namespace Servel.NET
{
    public static class PathStringExtensions
    {
        public static bool IsRoot(this PathString pathString)
        {
            return pathString == "/";
        }
    }
}
