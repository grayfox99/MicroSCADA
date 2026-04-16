using MicroSCADA_Client.Models;

namespace MicroSCADA_Client.Services;

public interface IOpcUaService : IAsyncDisposable
{
    bool IsConnected { get; }
    event Action<string, string>? DataChanged;
    Task ConnectAsync(string endpointUrl);
    Task DisconnectAsync();
    Task<List<OpcNode>> BrowseAsync(string? nodeId = null);
    Task SubscribeAsync(IEnumerable<string> nodeIds);
}
