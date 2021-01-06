using Cistern.ValueLinq.Nodes;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    struct ToLookup<TSource, TKey>
        : IForwardEnumerator<TSource>
    {
        Lookup<TKey, TSource> _lookup;
        Func<TSource, TKey> _keySelector;

        public ToLookup(IEqualityComparer<TKey> comparer, Func<TSource, TKey> keySelector)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            (_keySelector, _lookup) = (keySelector, GroupByNode.CreateLookup<TSource, TKey>(comparer));
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }

        public TResult GetResult<TResult>() => (TResult)(object)GetResult();
        public Lookup<TKey, TSource> GetResult() => _lookup;

        public bool ProcessNext(TSource input)
        {
            _lookup.GetGrouping(_keySelector(input), true).Add(input);
            return true;
        }
    }

    struct ToLookup<TSource, TKey, TElement>
        : IForwardEnumerator<TSource>
    {
        Lookup<TKey, TElement> _lookup;
        Func<TSource, TKey> _keySelector;
        Func<TSource, TElement> _elementSelector;

        public ToLookup(IEqualityComparer<TKey> comparer, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            (_keySelector, _elementSelector, _lookup) = (keySelector, elementSelector, GroupByNode.CreateLookup<TElement, TKey>(comparer));
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }

        public TResult GetResult<TResult>() => (TResult)(object)GetResult();
        public Lookup<TKey, TElement> GetResult() => _lookup;

        public bool ProcessNext(TSource input)
        {
            _lookup.GetGrouping(_keySelector(input), true).Add(_elementSelector(input));
            return true;
        }
    }
}
