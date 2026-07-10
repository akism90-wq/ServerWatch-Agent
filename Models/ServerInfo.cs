namespace ServerWatchAgent.Models;

public sealed class ServerInfo
{
    public string Name { get; init; } = string.Empty;

    public bool Online { get; init; }

    public string OperatingSystem { get; init; } = string.Empty;
}
