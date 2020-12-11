using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    struct ToDictionary<T, TKey>
        : IForwardEnumerator<T>
    {
        private Dictionary<TKey, T> _dictionary;
        private Func<T, TKey> _keySelector;

        public ToDictionary(Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            (_keySelector, _dictionary) = (keySelector, new Dictionary<TKey, T>(comparer));
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public Dictionary<TKey, T> GetResult() => _dictionary;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _dictionary.Add(_keySelector(input), input);
            return true;
        }
    }

    struct ToDictionary<T, TKey, TValue>
        : IForwardEnumerator<T>
    {
        private Dictionary<TKey, TValue> _dictionary;
        private Func<T, TKey> _keySelector;
        private Func<T, TValue> _elementSelector;

        public ToDictionary(Func<T, TKey> keySelector, Func<T, TValue> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            (_keySelector, _elementSelector, _dictionary) = (keySelector, elementSelector, new Dictionary<TKey, TValue>(comparer));
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public Dictionary<TKey, TValue> GetResult() => _dictionary;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _dictionary.Add(_keySelector(input), _elementSelector(input));
            return true;
        }
    }
}
