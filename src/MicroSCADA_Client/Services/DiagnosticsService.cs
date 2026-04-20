namespace MicroSCADA_Client.Services;

public sealed class DiagnosticsService : IDiagnosticsService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1);

    private readonly IOpcUaService _opc;
    private readonly Timer _timer;
    private DiagnosticsSnapshot _current = Empty();

    public DiagnosticsSnapshot Current => _current;
    public event Action<DiagnosticsSnapshot>? Updated;

    public DiagnosticsService(IOpcUaService opc)
    {
        _opc = opc;
        _opc.Connected += OnConnected;
        _opc.Disconnected += OnDisconnected;
        _opc.SessionFaulted += OnFaulted;
        _timer = new Timer(Tick, null, TickInterval, TickInterval);
    }

    private void OnConnected() => Publish();
    private void OnDisconnected() => Publish();
    private void OnFaulted(string _) => Publish();
    private void Tick(object? _) => Publish();

    private void Publish()
    {
        var state = ResolveState();
        var uptime = _opc.ConnectedSinceUtc.HasValue
            ? DateTime.UtcNow - _opc.ConnectedSinceUtc.Value
            : (TimeSpan?)null;

        var snapshot = new DiagnosticsSnapshot(
            state,
            _opc.CurrentEndpointUrl,
            _opc.SecurityPolicyUri,
            uptime,
            _opc.MonitoredItemCount,
            _opc.PublishingIntervalMs,
            _opc.LastKeepAliveUtc,
            _opc.KeepAliveLatencyMs,
            _opc.MissedKeepAliveCount,
            _opc.LastNotificationUtc,
            _opc.LastError);

        _current = snapshot;
        Updated?.Invoke(snapshot);
    }

    private ConnectionState ResolveState()
    {
        if (!_opc.IsConnected) return ConnectionState.Disconnected;
        return _opc.MissedKeepAliveCount > 0 ? ConnectionState.Faulted : ConnectionState.Connected;
    }

    private static DiagnosticsSnapshot Empty() => new(
        ConnectionState.Disconnected, null, null, null, 0, null, null, null, 0, null, null);

    public void Dispose()
    {
        _timer.Dispose();
        _opc.Connected -= OnConnected;
        _opc.Disconnected -= OnDisconnected;
        _opc.SessionFaulted -= OnFaulted;
    }
}
