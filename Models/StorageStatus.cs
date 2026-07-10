namespace ServerWatchAgent.Models;

public sealed class StorageStatus
{
    public string Path { get; init; } = string.Empty;

    public int UsedPercent { get; init; }

    public double FreeGb { get; init; }

    public bool Warning { get; init; }
}
