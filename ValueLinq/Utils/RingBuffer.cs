using System;

namespace Cistern.ValueLinq.Utils
{
    // Holds the last `capacity` elements added. It fills in order, growing toward the capacity rather than allocating it,
    // as a capacity can be as large as int.MaxValue against a short source; once full, each add overwrites, and hands
    // back, the oldest element, so the steady state is one load and one store per element.
    struct RingBuffer<T>
    {
        private const int DefaultInitialSize = 4;

        private T[]? _items;
        private int _head;      // the oldest element once full; 0 while filling
        private int _count;
        private readonly int _capacity;
        private readonly long _bound;

        // A source's maximum length is a bound that can be loose (and is wrong once the source has changed), so it only
        // ever limits the sizes grown to, never sets them: a short source is buffered in one allocation of its length,
        // and one that outgrows its bound grows on toward the capacity.
        public RingBuffer(int capacity, long? maximumLength)
        {
            _items = null;
            _head = 0;
            _count = 0;
            _capacity = capacity;
            _bound = Math.Max(1, maximumLength ?? long.MaxValue);
        }

        public readonly int Count => _count;

        // true when the buffer was full, with the element that left it; a zero capacity hands back every element
        public bool Add(T item, [MaybeNullWhen(false)] out T evicted)
        {
            if (_count < _capacity)
            {
                var items = _items;
                if (items is null || _count == items.Length)
                    items = Grow();
                items[_count++] = item;
                evicted = default;
                return false;
            }

            var full = _items;
            if (full is null)
            {
                evicted = item;
                return true;
            }

            evicted = full[_head];
            full[_head] = item;
            if (++_head == full.Length)
                _head = 0;
            return true;
        }

        private T[] Grow()
        {
            var items = _items;
            var limit = _count < _bound ? Math.Min(_capacity, _bound) : _capacity;
            var size = (int)Math.Min(items is null ? DefaultInitialSize : (long)items.Length * 2, limit);
            var grown = new T[size];
            if (_count > 0)
                Array.Copy(items!, grown, _count);
            return _items = grown;
        }

        // the elements oldest first, at the start of the returned array, which is null when nothing was added; the
        // buffer is rotated in place, so it allocates nothing
        public T[]? Linearize()
        {
            var items = _items;
            if (items is not null && _head != 0)
            {
                var span = items.AsSpan(0, _count);
                span[.._head].Reverse();
                span[_head..].Reverse();
                span.Reverse();
                _head = 0;
            }
            return items;
        }
    }
}
