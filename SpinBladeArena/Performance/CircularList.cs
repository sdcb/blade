using System.Collections;

namespace SpinBladeArena.Performance;

public class CircularList<T>(int capacity) : IList<T>
{
    readonly T[] _data = new T[capacity];
    int _tail;

    public int Count { get; private set; }

    private int NextIndex(int id) => (id + 1) % _data.Length;

    public void Add(T val)
    {
        _data[_tail] = val;
        _tail = NextIndex(_tail);
        Count = Math.Min(_data.Length, Count + 1);
    }

    private int Head => (_tail - Count) switch
    {
        < 0 => _tail - Count + _data.Length,
        { } x => x
    };

    public int IndexOf(T item)
    {
        int head = Head;
        for (int i = 0; i < Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_data[(head + i) % _data.Length], item))
            {
                return i;
            }
        }
        return -1; // not found
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (Count == _data.Length)
            throw new InvalidOperationException("CircularList is full");

        int insertPos = (Head + index) % _data.Length;
        for (int i = Count; i > index; i--)
        {
            int curr = (Head + i) % _data.Length;
            int prev = (Head + i - 1) % _data.Length;
            _data[curr] = _data[prev];
        }
        _data[insertPos] = item;
        _tail = NextIndex(_tail);
        Count++;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        for (int i = index; i < Count - 1; i++)
        {
            int curr = (Head + i) % _data.Length;
            int next = (Head + i + 1) % _data.Length;
            _data[curr] = _data[next];
        }
        _tail = (_tail - 1 + _data.Length) % _data.Length;
        Count--;
    }

    public void Clear()
    {
        _tail = 0;
        Count = 0;
    }

    public bool Contains(T item)
    {
        return IndexOf(item) != -1;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count) throw new ArgumentException("Insufficient space in target array");

        int head = Head;
        for (int i = 0; i < Count; i++)
        {
            array[arrayIndex + i] = _data[(head + i) % _data.Length];
        }
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index == -1)
            return false;
        RemoveAt(index);
        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        int head = Head;
        for (int i = 0; i < Count; i++)
        {
            yield return _data[(head + i) % _data.Length];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool IsReadOnly => false;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _data[(Head + index) % _data.Length];
        }
        set
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            _data[(Head + index) % _data.Length] = value;
        }
    }

    public T this[Index index]
    {
        get
        {
            if (index.IsFromEnd) index = new Index(Count - index.Value);
            return this[index.Value];
        }
        set
        {
            if (index.IsFromEnd) index = new Index(Count - index.Value);
            this[index.Value] = value;
        }
    }
}