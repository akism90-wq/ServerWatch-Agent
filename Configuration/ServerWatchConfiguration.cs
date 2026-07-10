using ServerWatchAgent.Models;

namespace ServerWatchAgent.Configuration;

public sealed class ServerWatchConfiguration
{
    public string StoragePath { get; init; } = "/";

    public double StorageWarningThresholdGb { get; init; }

    public QBittorrentConfiguration QBittorrent { get; set; } = new();

    public List<ServiceConfiguration> Services { get; init; } = [];
}