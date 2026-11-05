using Cistern.ValueLinq.Utils;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    readonly partial struct CountByKey<T, TKey>
        : IPushEnumerator<T>
        where TKey : notnull
    {
        private readonly InsertionMap<TKey, int> _counts;
        private readonly Func<T, TKey> _keySelector;

        public CountByKey(Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            _keySelector = keySelector;
            _counts = new InsertionMap<TKey, int>(comparer);
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var counts = _counts;
            var keySelector = _keySelector;

            foreach (var item in span)
                Increment(counts, keySelector(item));

            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public InsertionMap<TKey, int> GetResult() => _counts;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            Increment(_counts, _keySelector(input));

            return true;
        }

        private static void Increment(InsertionMap<TKey, int> counts, TKey key)
        {
            ref var count = ref counts.GetValueRefOrAddDefault(key, out _);
            count = checked(count + 1);
        }
    }
}
