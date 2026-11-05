using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    readonly partial struct ToDictionary<T, TKey>
        : IPushEnumerator<T>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, T> _dictionary;
        private readonly Func<T, TKey> _keySelector;

        public ToDictionary(Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer, int capacity)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            _keySelector = keySelector;
            _dictionary = new Dictionary<TKey, T>(capacity, comparer);
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public Dictionary<TKey, T> GetResult() => _dictionary;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _dictionary.Add(_keySelector(input), input);
            return true;
        }
    }

    [Aggregator]
    readonly partial struct ToDictionary<T, TKey, TValue>
        : IPushEnumerator<T>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly Func<T, TKey> _keySelector;
        private readonly Func<T, TValue> _elementSelector;

        public ToDictionary(Func<T, TKey> keySelector, Func<T, TValue> elementSelector, IEqualityComparer<TKey>? comparer, int capacity)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            _keySelector = keySelector;
            _elementSelector = elementSelector;
            _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public Dictionary<TKey, TValue> GetResult() => _dictionary;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _dictionary.Add(_keySelector(input), _elementSelector(input));
            return true;
        }
    }

    [Aggregator]
    readonly partial struct ToDictionaryFromKeyValuePairs<TKey, TValue>
        : IPushEnumerator<KeyValuePair<TKey, TValue>>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public ToDictionaryFromKeyValuePairs(IEqualityComparer<TKey>? comparer, int capacity) => _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);

        static BatchKinds IPushEnumerator<KeyValuePair<TKey, TValue>>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;
        bool IPushEnumerator<KeyValuePair<TKey, TValue>>.ProcessBatch(scoped ref Batch<KeyValuePair<TKey, TValue>> batch)
        {
            var span = batch.GetSpan();
            var dictionary = _dictionary;

            // only the first span sizes it: EnsureCapacity grows to exactly the size asked, so a call per span would rehash each time
            if (dictionary.Count == 0)
                dictionary.EnsureCapacity(span.Length);

            foreach (var (key, value) in span)
                dictionary.Add(key, value);

            return true;
        }

        TPushResult IPushEnumerator<KeyValuePair<TKey, TValue>>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public Dictionary<TKey, TValue> GetResult() => _dictionary;

        bool IPushEnumerator<KeyValuePair<TKey, TValue>>.ProcessNext(KeyValuePair<TKey, TValue> input)
        {
            _dictionary.Add(input.Key, input.Value);

            return true;
        }
    }

    [Aggregator]
    readonly partial struct ToDictionaryFromTuples<TKey, TValue>
        : IPushEnumerator<(TKey Key, TValue Value)>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public ToDictionaryFromTuples(IEqualityComparer<TKey>? comparer, int capacity) => _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);

        static BatchKinds IPushEnumerator<(TKey Key, TValue Value)>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;
        bool IPushEnumerator<(TKey Key, TValue Value)>.ProcessBatch(scoped ref Batch<(TKey Key, TValue Value)> batch)
        {
            var span = batch.GetSpan();
            var dictionary = _dictionary;

            // only the first span sizes it: EnsureCapacity grows to exactly the size asked, so a call per span would rehash each time
            if (dictionary.Count == 0)
                dictionary.EnsureCapacity(span.Length);

            foreach (var (key, value) in span)
                dictionary.Add(key, value);

            return true;
        }

        TPushResult IPushEnumerator<(TKey Key, TValue Value)>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public Dictionary<TKey, TValue> GetResult() => _dictionary;

        bool IPushEnumerator<(TKey Key, TValue Value)>.ProcessNext((TKey Key, TValue Value) input)
        {
            _dictionary.Add(input.Key, input.Value);

            return true;
        }
    }
}
