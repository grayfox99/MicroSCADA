namespace OpticUA.Client.Services;

public enum ConnectionState
{
    Disconnected,
    Connected,
    Faulted
}

public sealed record DiagnosticsSnapshot(
    ConnectionState ConnectionState,
    string? EndpointUrl,
    string? SecurityPolicyUri,
    TimeSpan? SessionUptime,
    int MonitoredItemCount,
    int? PublishingIntervalMs,
    DateTime? LastKeepAliveUtc,
    double? KeepAliveLatencyMs,
    int MissedKeepAliveCount,
    DateTime? LastNotificationUtc,
    string? LastError);

public interface IDiagnosticsService : IDisposable
{
    DiagnosticsSnapshot Current { get; }
    event Action<DiagnosticsSnapshot>? Updated;
}
