using System;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct FoldForward<T, TAccumulate>
        : IPushEnumerator<T>
    {
        private TAccumulate _accumulate;
        private Func<TAccumulate, T, TAccumulate> _func;

        public FoldForward(Func<TAccumulate, T, TAccumulate> func, TAccumulate seed)
        {
            _func = func;
            _accumulate = seed;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var accumulate = _accumulate;
            var func = _func;
            foreach (var item in span)
                accumulate = func(accumulate, item);

            _accumulate = accumulate;

            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<TAccumulate, TPushResult>(GetResult());

        public TAccumulate GetResult() => _accumulate;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _accumulate = _func(_accumulate, input);
            return true;
        }
    }
}
