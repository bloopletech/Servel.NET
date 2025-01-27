using System.Collections.Concurrent;
using Servel.NET.FileProviders;
using Servel.NET.Models;

namespace Servel.NET.Services;

public class ThumbnailService(CacheDatabaseService databaseService)
{
    private readonly ThumbnailGenerator thumbnailGenerator = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> pathLocks = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> driveLocks = new();

    public static async Task ThumbnailDirectoryBackground(IBackgroundTaskQueue queue, ListingDirectoryInfo directoryInfo)
    {
        await queue.QueueAsync(async (services, _) =>
        {
            foreach(var physicalFileInfo in directoryInfo.Files)
            {
                await ThumbnailFileBackground(queue, physicalFileInfo);
            }
        });
    }

    public static async Task ThumbnailFileBackground(IBackgroundTaskQueue queue, ListingFileInfo fileInfo)
    {
        await queue.QueueAsync(async (services, _) =>
        {
            var thumbnailsService = services.GetRequiredService<ThumbnailService>();
            await thumbnailsService.FindOrCreateByPath(fileInfo);
        });
    }

    public async Task<byte[]?> FindOrCreateByPath(ListingFileInfo fileInfo)
    {
        if(FindByPath(fileInfo, out var existingData)) return existingData;

        var path = fileInfo.PhysicalPath;
        var pathLock = pathLocks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
        await pathLock.WaitAsync();

        try
        {
            return await FindOrCreateByPathRacy(fileInfo);
        }
        finally
        {
            pathLocks.TryRemove(path, out _);
            pathLock.Release();
        }
    }

    private async Task<byte[]?> FindOrCreateByPathRacy(ListingFileInfo fileInfo)
    {
        if(FindByPath(fileInfo, out var existingData)) return existingData;

        var data = await GenerateThumbnail(fileInfo.PhysicalPath);

        var thumbnail = new Thumbnail
        {
            Path = fileInfo.PhysicalPath,
            Data = data,
            Mtime = fileInfo.LastModified.ToUnixTimeMilliseconds()
        };

        using var db = databaseService.Connect();
        thumbnail.Save(db);

        return data;
    }

    private bool FindByPath(ListingFileInfo fileInfo, out byte[]? data)
    {
        using var db = databaseService.Connect();
        var existing = Thumbnail.FindByPath(db, fileInfo.PhysicalPath);
        if(existing != null && existing.Mtime == fileInfo.LastModified.ToUnixTimeMilliseconds())
        {
            data = existing.Data;
            return true;
        }

        data = null;
        return false;
    }

    private async Task<byte[]?> GenerateThumbnail(string path)
    {
        var pathRoot = Path.GetPathRoot(path) ?? "";
        var driveLock = driveLocks.GetOrAdd(pathRoot, _ => new SemaphoreSlim(1, 1));
        await driveLock.WaitAsync();

        try
        {
            return await thumbnailGenerator.Thumbnail(path);
        }
        finally
        {
            driveLocks.TryRemove(pathRoot, out _);
            driveLock.Release();
        }
    }
}
