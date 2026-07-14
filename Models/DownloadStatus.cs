namespace ServerWatchAgent.Models;

public sealed class DownloadStatus
{
    public string Name { get; init; } = string.Empty;

    public double SizeGb { get; init; }

    public int ProgressPercent { get; init; }

    public long DownloadSpeedBytesPerSecond { get; init; }

    public long EtaSeconds { get; init; }

    public double AmountLeftGb { get; init; }

    public string State { get; init; } = string.Empty;

    public bool Suspicious { get; init; }
}