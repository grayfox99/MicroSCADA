using System.Collections.Concurrent;
using System.Globalization;

namespace OpticUA.Client.Services;

public sealed class TagHistoryService : ITagHistoryService
{
    private const int BufferCapacity = 300;
    private static readonly TimeSpan RedrawInterval = TimeSpan.FromSeconds(1);

    private readonly IOpcUaService _opc;
    private readonly ConcurrentDictionary<string, RingBuffer<TagSample>> _buffers = new();
    private readonly Timer _redrawTimer;
    private int _pendingUpdate;

    public event Action? HistoryUpdated;

    public IReadOnlyCollection<string> TrackedNodeIds => _buffers.Keys.ToArray();

    public TagHistoryService(IOpcUaService opc)
    {
        _opc = opc;
        _opc.DataChanged += OnDataChanged;
        _redrawTimer = new Timer(OnRedrawTick, null, RedrawInterval, RedrawInterval);
    }

    public void Track(string nodeId)
        => _buffers.TryAdd(nodeId, new RingBuffer<TagSample>(BufferCapacity));

    public void Untrack(string nodeId)
        => _buffers.TryRemove(nodeId, out _);

    public IReadOnlyList<TagSample> GetWindow(string nodeId, TimeSpan window)
    {
        if (!_buffers.TryGetValue(nodeId, out var buffer))
            return Array.Empty<TagSample>();

        var cutoff = DateTime.UtcNow - window;
        var snapshot = buffer.Snapshot();
        var firstInWindow = 0;
        while (firstInWindow < snapshot.Count && snapshot[firstInWindow].Ts < cutoff)
            firstInWindow++;

        if (firstInWindow == 0) return snapshot;
        var trimmed = new TagSample[snapshot.Count - firstInWindow];
        for (int i = 0; i < trimmed.Length; i++)
            trimmed[i] = snapshot[firstInWindow + i];
        return trimmed;
    }

    private void OnDataChanged(string nodeId, string value)
    {
        if (!_buffers.TryGetValue(nodeId, out var buffer))
            return;

        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var numeric))
            return;

        buffer.Add(new TagSample(DateTime.UtcNow, numeric));
        Interlocked.Exchange(ref _pendingUpdate, 1);
    }

    private void OnRedrawTick(object? state)
    {
        if (Interlocked.Exchange(ref _pendingUpdate, 0) == 1)
            HistoryUpdated?.Invoke();
    }

    public void Dispose()
    {
        _redrawTimer.Dispose();
        _opc.DataChanged -= OnDataChanged;
    }
}
