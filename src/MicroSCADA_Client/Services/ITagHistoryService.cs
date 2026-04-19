namespace MicroSCADA_Client.Services;

public readonly record struct TagSample(DateTime Ts, double Value);

public interface ITagHistoryService : IDisposable
{
    void Track(string nodeId);
    void Untrack(string nodeId);
    IReadOnlyList<TagSample> GetWindow(string nodeId, TimeSpan window);
    IReadOnlyCollection<string> TrackedNodeIds { get; }
    event Action? HistoryUpdated;
}
