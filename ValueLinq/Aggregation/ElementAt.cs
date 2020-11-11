using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct ElementAt<T>
        : IForwardEnumerator<T>
    {
        private readonly int _elementAtIndex;

        private int _index;
        private bool _found;
        private T _elementAt;

        public ElementAt(int index, int? size)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            (_elementAtIndex, _index, _found, _elementAt) = (index, index >= size ? index : - 1, false, default);
        }

        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>()
        {
            if (!_found)
                throw new ArgumentOutOfRangeException("index");

            return (TResult)(object)_elementAt;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
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

    struct ElementAtOrDefault<T>
        : IForwardEnumerator<T>
    {
        private readonly int _elementAtIndex;

        private int _index;
        private T _elementAt;

        public ElementAtOrDefault(int index, int? size) => (_elementAtIndex, _index, _elementAt) = (index, (index < 0 || index >= size) ? index : - 1, default);

        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_elementAt;

        bool IForwardEnumerator<T>.ProcessNext(T input)
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
