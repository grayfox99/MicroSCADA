namespace OpticUA.Client.Services;

public sealed class RingBuffer<T>
{
    private readonly T[] _items;
    private readonly object _lock = new();
    private int _head;
    private int _count;

    public int Capacity { get; }

    public RingBuffer(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        Capacity = capacity;
        _items = new T[capacity];
    }

    public void Add(T item)
    {
        lock (_lock)
        {
            _items[_head] = item;
            _head = (_head + 1) % Capacity;
            if (_count < Capacity) _count++;
        }
    }

    public IReadOnlyList<T> Snapshot()
    {
        lock (_lock)
        {
            var result = new T[_count];
            var start = _count < Capacity ? 0 : _head;
            for (int i = 0; i < _count; i++)
                result[i] = _items[(start + i) % Capacity];
            return result;
        }
    }
}
