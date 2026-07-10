using System.Text.Json.Serialization;

namespace ServerWatchAgent.Models;

public sealed class QBittorrentTorrent
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long SizeBytes { get; set; }

    [JsonPropertyName("progress")]
    public double Progress { get; set; }

    [JsonPropertyName("dlspeed")]
    public long DownloadSpeedBytesPerSecond { get; set; }

    [JsonPropertyName("eta")]
    public long EtaSeconds { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("amount_left")]
    public long AmountLeftBytes { get; set; }
}