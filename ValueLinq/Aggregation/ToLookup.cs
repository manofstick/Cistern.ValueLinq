using Cistern.ValueLinq.Nodes;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    readonly partial struct ToLookup<TSource, TKey>
        : IPushEnumerator<TSource>
    {
        readonly Nodes.GroupBy.Lookup<TKey, TSource> _lookup;
        readonly Func<TSource, TKey> _keySelector;

        public ToLookup(IEqualityComparer<TKey>? comparer, Func<TSource, TKey> keySelector)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            _keySelector = keySelector;
            _lookup = Nodes.GroupBy.CreateLookup<TSource, TKey>(comparer);
        }

        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)GetResult();
        public Nodes.GroupBy.Lookup<TKey, TSource> GetResult() => _lookup;

        public bool ProcessNext(TSource input)
        {
            _lookup.GetGrouping(_keySelector(input), true)!.Add(input);
            return true;
        }
    }

    // a join's lookup over its inner sequence, which leaves out the elements whose key is null
    [Aggregator]
    readonly partial struct ToJoinLookup<TSource, TKey>
        : IPushEnumerator<TSource>
    {
        readonly Nodes.GroupBy.Lookup<TKey, TSource> _lookup;
        readonly Func<TSource, TKey> _keySelector;

        public ToJoinLookup(IEqualityComparer<TKey>? comparer, Func<TSource, TKey> keySelector)
        {
            _keySelector = keySelector;
            _lookup = Nodes.GroupBy.CreateLookup<TSource, TKey>(comparer);
        }


        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)_lookup;

        public bool ProcessNext(TSource input)
        {
            var key = _keySelector(input);
            if (key is not null)
                _lookup.GetGrouping(key, true)!.Add(input);

            return true;
        }
    }

    // a join's table over its inner sequence, sized once from a container's span
    [Aggregator]
    readonly partial struct ToJoinTable<TSource, TKey>
        : IPushEnumerator<TSource>
    {
        readonly Nodes.Join.Table<TKey, TSource> _table;
        readonly Func<TSource, TKey> _keySelector;

        public ToJoinTable(IEqualityComparer<TKey>? comparer, Func<TSource, TKey> keySelector, bool keepsNullKeys)
        {
            _keySelector = keySelector;
            _table = new Nodes.Join.Table<TKey, TSource>(comparer, keepsNullKeys);
        }

        static BatchKinds IPushEnumerator<TSource>.AllowBatchesWhen => BatchKinds.Container;

        bool IPushEnumerator<TSource>.ProcessBatch(scoped ref Batch<TSource> batch)
        {
            var span = batch.GetSpan();
            var table = _table;
            var keySelector = _keySelector;

            table.EnsureCapacity(table._elementCount + span.Length);
            foreach (var element in span)
                table.Add(keySelector(element), element);

            return true;
        }

        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)_table;

        public bool ProcessNext(TSource input)
        {
            _table.Add(_keySelector(input), input);

            return true;
        }
    }

    [Aggregator]
    readonly partial struct ToLookup<TSource, TKey, TElement>
        : IPushEnumerator<TSource>
    {
        readonly Nodes.GroupBy.Lookup<TKey, TElement> _lookup;
        readonly Func<TSource, TKey> _keySelector;
        readonly Func<TSource, TElement> _elementSelector;

        public ToLookup(IEqualityComparer<TKey>? comparer, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            _keySelector = keySelector;
            _elementSelector = elementSelector;
            _lookup = Nodes.GroupBy.CreateLookup<TElement, TKey>(comparer);
        }

        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)GetResult();
        public Nodes.GroupBy.Lookup<TKey, TElement> GetResult() => _lookup;

        public bool ProcessNext(TSource input)
        {
            _lookup.GetGrouping(_keySelector(input), true)!.Add(_elementSelector(input));
            return true;
        }
    }
}
