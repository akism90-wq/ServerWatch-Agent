using ServerWatchAgent.Models;

namespace ServerWatchAgent.Configuration;

public sealed class ServerWatchConfiguration
{
    public string StoragePath { get; init; } = "/";

    public double StorageWarningThresholdGb { get; init; }

    public string ApiKey { get; init; } = string.Empty;

    public QBittorrentConfiguration QBittorrent { get; set; } = new();

    public List<ServiceConfiguration> Services { get; init; } = [];

    public DownloadWarningConfiguration DownloadWarnings { get; set; } = new();
}