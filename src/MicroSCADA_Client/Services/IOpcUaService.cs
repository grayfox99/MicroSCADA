using MicroSCADA_Client.Models;

namespace MicroSCADA_Client.Services;

public interface IOpcUaService : IAsyncDisposable
{
    bool IsConnected { get; }
    string? CurrentEndpointUrl { get; }
    string? SecurityPolicyUri { get; }
    DateTime? ConnectedSinceUtc { get; }
    int? PublishingIntervalMs { get; }
    int MonitoredItemCount { get; }
    DateTime? LastKeepAliveUtc { get; }
    double? KeepAliveLatencyMs { get; }
    int MissedKeepAliveCount { get; }
    DateTime? LastNotificationUtc { get; }
    string? LastError { get; }

    event Action<string, string>? DataChanged;
    event Action? Connected;
    event Action? Disconnected;
    event Action<string>? SessionFaulted;

    Task ConnectAsync(string endpointUrl);
    Task DisconnectAsync();
    Task<List<OpcNode>> BrowseAsync(string? nodeId = null);
    Task SubscribeAsync(IEnumerable<string> nodeIds);
}
