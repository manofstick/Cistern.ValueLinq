using Cistern.ValueLinq.Utils;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    readonly partial struct AggregateByKey<T, TKey, TAccumulate>
        : IPushEnumerator<T>
        where TKey : notnull
    {
        private readonly InsertionMap<TKey, TAccumulate> _accumulates;
        private readonly Func<T, TKey> _keySelector;
        private readonly TAccumulate _seed;
        private readonly Func<TKey, TAccumulate>? _seedSelector;
        private readonly Func<TAccumulate, T, TAccumulate> _func;

        public AggregateByKey(Func<T, TKey> keySelector, TAccumulate seed, Func<TKey, TAccumulate>? seedSelector, Func<TAccumulate, T, TAccumulate> func, IEqualityComparer<TKey>? comparer)
        {
            _keySelector = keySelector;
            _seed = seed;
            _seedSelector = seedSelector;
            _func = func;
            _accumulates = new InsertionMap<TKey, TAccumulate>(comparer);
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var accumulates = _accumulates;
            var keySelector = _keySelector;
            var seed = _seed;
            var seedSelector = _seedSelector;
            var func = _func;

            foreach (var item in span)
                Fold(accumulates, keySelector, seed, seedSelector, func, item);

            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public InsertionMap<TKey, TAccumulate> GetResult() => _accumulates;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            Fold(_accumulates, _keySelector, _seed, _seedSelector, _func, input);

            return true;
        }

        private static void Fold(InsertionMap<TKey, TAccumulate> accumulates, Func<T, TKey> keySelector, TAccumulate seed, Func<TKey, TAccumulate>? seedSelector, Func<TAccumulate, T, TAccumulate> func, T item)
        {
            var key = keySelector(item);

            ref var accumulate = ref accumulates.GetValueRefOrAddDefault(key, out var exists);
            if (!exists)
                accumulate =
                    seedSelector is null
                        ? seed
                        : seedSelector(key);

            accumulate = func(accumulate!, item);
        }
    }
}
