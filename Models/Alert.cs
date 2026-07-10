namespace ServerWatchAgent.Models;

public sealed class Alert
{
    public AlertSeverity Severity { get; init; }

    public string Message { get; init; } = string.Empty;
}

public enum AlertSeverity
{
    Information,
    Warning,
    Problem
}
