namespace ServerWatchAgent.Models;

public sealed class ServiceHealth
{
    public string Name { get; init; } = string.Empty;

    public ServiceState State { get; init; } = ServiceState.Unknown;

    public string Detail { get; init; } = string.Empty;
}
