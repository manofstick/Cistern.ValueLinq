using System;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct ElementAt<T>
        : IPushEnumerator<T>
    {
        private readonly int _elementAtIndex;

        private int _index;
        private bool _found;
        private T? _elementAt;

        public ElementAt(int index, int? size)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            _elementAtIndex = index;
            _index = index >= size ? index : - 1;
            _found = false;
            _elementAt = default;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (_index >= _elementAtIndex)
                return false;

            var offset = _elementAtIndex - _index - 1;
            if (offset < span.Length)
            {
                _index = _elementAtIndex;
                _found = true;
                _elementAt = span[offset];
                return false;
            }

            _index += span.Length;
            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>()
        {
            if (!_found)
                throw new ArgumentOutOfRangeException("index");

            return Impl.ResultCast<T?, TPushResult>(_elementAt);
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_index >= _elementAtIndex-1)
            {
                if (++_index == _elementAtIndex)
                {
                    _found = true;
                    _elementAt = input;
                }
                return false;
            }
            ++_index;
            return true;
        }
    }

    // The element at an index from the start, or the last (^1), as (found, element): the answer to the ElementAt request
    // a node gives by running its source when the source declines (Select, which then projects only the element found).
    [Aggregator]
    partial struct FoundElementAt<T>
        : IPushEnumerator<T>
    {
        // -1 for the last
        private readonly int _target;

        private int _index;
        private bool _found;
        private T? _element;

        public FoundElementAt(Index index)
        {
            _target = index.IsFromEnd ? -1 : index.Value;
            _index = -1;
            _found = false;
            _element = default;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (_target < 0)
            {
                if (!span.IsEmpty)
                    (_found, _element) = (true, span[^1]);
                return true;
            }

            var offset = _target - _index - 1;
            if (offset < span.Length)
            {
                (_index, _found, _element) = (_target, true, span[offset]);
                return false;
            }

            _index += span.Length;
            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<(bool, T), TPushResult>((_found, _element!));

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_target < 0)
            {
                (_found, _element) = (true, input);
                return true;
            }

            if (++_index < _target)
                return true;

            (_found, _element) = (true, input);
            return false;
        }
    }

    // The element at an index from the start as (found, element, count), the count being the number of elements passed
    // over when the sequence is shorter, so a Concat carries the index into its next part.
    [Aggregator]
    partial struct ElementAtOrCount<T>
        : IPushEnumerator<T>
    {
        private readonly int _target;

        private int _count;
        private bool _found;
        private T? _element;

        public ElementAtOrCount(int index)
        {
            _target = index;
            _count = 0;
            _found = false;
            _element = default;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var offset = _target - _count;
            if (offset < span.Length)
            {
                (_found, _element) = (true, span[offset]);
                return false;
            }

            _count += span.Length;
            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<(bool, T, int), TPushResult>((_found, _element!, _count));

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_count < _target)
            {
                ++_count;
                return true;
            }

            (_found, _element) = (true, input);
            return false;
        }
    }

    [Aggregator]
    partial struct ElementAtOrDefault<T>
        : IPushEnumerator<T>
    {
        private readonly int _elementAtIndex;

        private int _index;
        private T? _elementAt;

        public ElementAtOrDefault(int index, int? size)
        {
            _elementAtIndex = index;
            _index = (index < 0 || index >= size) ? index : - 1;
            _elementAt = default;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (_index >= _elementAtIndex)
                return false;

            var offset = _elementAtIndex - _index - 1;
            if (offset < span.Length)
            {
                _index = _elementAtIndex;
                _elementAt = span[offset];
                return false;
            }

            _index += span.Length;
            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T?, TPushResult>(_elementAt);

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_index >= _elementAtIndex - 1)
            {
                if (++_index == _elementAtIndex)
                {
                    _elementAt = input;
                }
                return false;
            }
            ++_index;
            return true;
        }
    }
}
