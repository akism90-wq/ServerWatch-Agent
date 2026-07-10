namespace ServerWatchAgent.Models;

public sealed class ServerStatus
{
    public ServerInfo Server { get; init; } = new();

    public StorageStatus Storage { get; init; } = new();

    public List<DownloadStatus> Downloads { get; init; } = [];

    public List<ServiceHealth> Services { get; init; } = [];

    public List<Alert> Alerts { get; init; } = [];
}
