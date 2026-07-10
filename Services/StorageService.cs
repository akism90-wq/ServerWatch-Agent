using ServerWatchAgent.Models;

namespace ServerWatchAgent.Services;

public sealed class StorageService
{
    public StorageStatus GetStatus(
        string storagePath,
        double warningThresholdGb)
    {
        var drive = new DriveInfo(storagePath);

        var totalBytes = drive.TotalSize;
        var freeBytes = drive.AvailableFreeSpace;
        var usedBytes = totalBytes - freeBytes;

        var freeGb = freeBytes / 1024.0 / 1024.0 / 1024.0;
        var usedPercent = (int)Math.Round(
            usedBytes * 100.0 / totalBytes
        );

        return new StorageStatus
        {
            Path = storagePath,
            UsedPercent = usedPercent,
            FreeGb = Math.Round(freeGb, 1),
            Warning = freeGb < warningThresholdGb
        };
    }
}
