using Microsoft.Extensions.FileProviders;

namespace Servel.NET
{
    public class EntryFactory
    {
        public static string[] IMAGE_EXTS = { ".jpg", ".jpeg", ".png", ".gif" };
        public static string[] VIDEO_EXTS = { ".webm", ".mp4", ".mkv" };
        public static string[] AUDIO_EXTS = { ".mp3", ".m4a", ".wav" };
        public static string[] TEXT_EXTS = { ".txt" };

        public static Entry Home(string href)
        {
            return new Entry
            {
                Ftype = Ftype.Directory,
                Type = "Dir",
                ListingClasses = "home directory",
                Icon = "🏠",
                Href = href,
                Name = "Listings Home"
            };
        }

        public static Entry Top(string href)
        {
            return new Entry
            {
                Ftype = Ftype.Directory,
                Type = "Dir",
                ListingClasses = "top directory",
                Icon = "🔝",
                Href = href,
                Name = "Top Directory"
            };
        }

        public static Entry Parent(string href)
        {
            return new Entry
            {
                Ftype = Ftype.Directory,
                Type = "Dir",
                ListingClasses = "parent directory",
                Icon = "⬆️",
                Href = href,
                Name = "Parent Directory"
            };
        }

        public static Entry? For(IFileInfo fileInfo)
        {
            try
            {
                return new EntryFactory(fileInfo).Entry();
            }
            catch (IOException e)
            {
                return null;
            }
        }

        public string PathBasename { get; init; }
        public string? PathExtname { get; init; }
        public DateTimeOffset PathMtime { get; init; }
        public long PathSize { get; init; }
        public Ftype PathFtype { get; init; }
        public bool PathDirectory { get; init; }
        public bool PathFile { get; init; }

        public EntryFactory(IFileInfo fileInfo)
        {
            PathBasename = fileInfo.Name;
            PathExtname = Path.GetExtension(fileInfo.Name)?.ToLower();
            PathMtime = fileInfo.LastModified;
            PathSize = fileInfo.Length;
            PathFtype = fileInfo.IsDirectory ? Ftype.Directory : Ftype.File;
            PathDirectory = PathFtype == Ftype.Directory;
            PathFile = PathFtype == Ftype.File;
        }

        public Entry Entry()
        {
            return new Entry
            {
                Ftype = PathFtype,
                Type = Type(),
                MediaType = MediaType(),
                ListingClasses = ListingClasses(),
                Icon = Icon(),
                Href = PathBasename,
                Name = PathBasename,
                Size = Size(),
                Mtime = PathMtime
            };
        }

        public string Type()
        {
            if (PathDirectory) return "Dir";
            if (PathFile) return PathExtname!.Replace(".", string.Empty);
            return "";
        }

        public MediaType? MediaType()
        {
            if (!PathFile || string.IsNullOrEmpty(PathExtname)) return null;

            if (IMAGE_EXTS.Contains(PathExtname)) return NET.MediaType.Image;
            if (VIDEO_EXTS.Contains(PathExtname)) return NET.MediaType.Video;
            if (AUDIO_EXTS.Contains(PathExtname)) return NET.MediaType.Audio;
            if (TEXT_EXTS.Contains(PathExtname)) return NET.MediaType.Text;
            return null;
        }

        public string ListingClasses()
        {
            var klasses = new List<string>();
            if (PathFile) klasses.Add("file");
            if (PathDirectory) klasses.Add("directory");
            if (MediaType() != null) klasses.Add("media");
            if (MediaType() != null) klasses.Add(MediaType()!.Value.ToString());
            return string.Join(" ", klasses);
        }

        public string Icon()
        {
            if (PathDirectory) return "📁";
            switch (MediaType())
            {
                case NET.MediaType.Video: return "🎞️";
                case NET.MediaType.Image: return "🖼️";
                case NET.MediaType.Audio: return "🔊";
                case NET.MediaType.Text: return "📝";
                case null: return "";
                default: return "";
            }
        }

        public long? Size()
        {
            return PathDirectory ? null : PathSize;
        }
    }
}
