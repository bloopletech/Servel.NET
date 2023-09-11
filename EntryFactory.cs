using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using System.Web;

namespace Servel.NET
{
    public class EntryFactory
    {
        public static string[] IMAGE_EXTS = { ".jpg", ".jpeg", ".png", ".gif" };
        public static string[] VIDEO_EXTS = { ".webm", ".mp4", ".mkv" };
        public static string[] AUDIO_EXTS = { ".mp3", ".m4a", ".wav" };
        public static string[] TEXT_EXTS = { ".txt" };

        public static readonly Entry HomeEntry = new Entry
        {
            Type = "Dir",
            Class = "home directory",
            Icon = "🏠",
            Href = "/",
            Name = "Listings Home",
            SizeText = "-",
            MtimeText = "-"
        };

        public static readonly Entry ParentEntry = new Entry
        {
            Type = "Dir",
            Class = "parent directory",
            Icon = "⬆️",
            Href = "../",
            Name = "Parent Directory",
            SizeText = "-",
            MtimeText = "-"
        };

        public static Entry Top(string href)
        {
            return new Entry
            {
                Type = "Dir",
                Class = "top directory",
                Icon = "🔝",
                Href = href,
                Name = "Top Directory",
                SizeText = "-",
                MtimeText = "-"
            };
        }

        public static Entry? For(IFileInfo fileInfo)
        {
            try
            {
                if(fileInfo is PhysicalFileInfo physicalFileInfo) return ForFile(physicalFileInfo);
                if(fileInfo is PhysicalDirectoryInfo physicalDirectoryInfo) return ForDirectory(physicalDirectoryInfo);
                throw new InvalidOperationException("Unexpected IFileInfo implementation");
            }
            catch (IOException e)
            {
                return null;
            }
        }

        private static Entry ForDirectory(PhysicalDirectoryInfo directoryInfo)
        {
            return new Entry
            {
                Type = "Dir",
                MediaType = null,
                Class = "directory",
                Icon = "📁",
                Href = HttpUtility.UrlPathEncode(directoryInfo.Name),
                Name = directoryInfo.Name,
                Size = 0,
                SizeText = "-",
                Mtime = directoryInfo.LastModified.ToUnixTimeMilliseconds(),
                MtimeText = directoryInfo.LastModified.ToString("d MMM yyyy h:mm tt"),
                Media = false
            };
        }

        private static Entry ForFile(PhysicalFileInfo fileInfo)
        {
            var fileExtension = Path.GetExtension(fileInfo.Name)?.ToLower();
            var mediaType = GetMediaType(fileExtension);
            return new Entry
            {
                Type = fileExtension!.Replace(".", string.Empty),
                MediaType = mediaType,
                Class = GetListingClasses(mediaType),
                Icon = GetIcon(mediaType),
                Href = HttpUtility.UrlPathEncode(fileInfo.Name),
                Name = fileInfo.Name,
                Size = fileInfo.Length,
                SizeText = fileInfo.Length.ToString(),
                Mtime = fileInfo.LastModified.ToUnixTimeMilliseconds(),
                MtimeText = fileInfo.LastModified.ToString("d MMM yyyy h:mm tt"),
                Media = mediaType != null
            };
        }

        private static string? GetMediaType(string? fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension)) return null;

            if (IMAGE_EXTS.Contains(fileExtension)) return "image";
            if (VIDEO_EXTS.Contains(fileExtension)) return "video";
            if (AUDIO_EXTS.Contains(fileExtension)) return "audio";
            if (TEXT_EXTS.Contains(fileExtension)) return "text";
            return null;
        }

        private static string GetListingClasses(string? mediaType)
        {
            var klasses = new List<string>
            {
                "file"
            };
            if (mediaType != null)
            {
                klasses.Add("media");
                klasses.Add(mediaType);
            }
            return string.Join(" ", klasses);
        }

        private static string GetIcon(string? mediaType)
        {
            switch (mediaType)
            {
                case "video": return "🎞️";
                case "image": return "🖼️";
                case "audio": return "🔊";
                case "text": return "📝";
                case null: return "";
                default: return "";
            }
        }
    }
}
