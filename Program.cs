using System.Runtime.InteropServices;
using ServerWatchAgent.Models;

var builder = WebApplication.CreateBuilder(args);

var storagePath =
    builder.Configuration["ServerWatch:StoragePath"]
    ?? throw new InvalidOperationException("ServerWatch:StoragePath is not configured.");

var storageWarningThresholdGb =
    builder.Configuration.GetValue<double>(
        "ServerWatch:StorageWarningThresholdGb"
    );

var app = builder.Build();

app.MapGet("/status", () =>
{
    var drive = new DriveInfo(storagePath);

    var totalBytes = drive.TotalSize;
    var freeBytes = drive.AvailableFreeSpace;
    var usedBytes = totalBytes - freeBytes;

    var freeGb = freeBytes / 1024.0 / 1024.0 / 1024.0;
    var usedPercent = (int)Math.Round(
        usedBytes * 100.0 / totalBytes
    );

    var status = new ServerStatus
    {
        Server = new ServerInfo
        {
            Name = Environment.MachineName,
            Online = true,
            OperatingSystem = RuntimeInformation.OSDescription
        },

        Storage = new StorageStatus
        {
            Path = storagePath,
            UsedPercent = usedPercent,
            FreeGb = Math.Round(freeGb, 1),
            Warning = freeGb < storageWarningThresholdGb
        }
    };

    return Results.Ok(status);
});

app.Run();
