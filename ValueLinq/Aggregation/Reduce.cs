using System;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct ReduceForward<T>
        : IPushEnumerator<T>
    {
        private bool _hasData;
        [AllowNull] private T _accumulate;
        private Func<T, T, T> _func;

        public ReduceForward(Func<T, T, T> func)
        {
            _func = func;
            _hasData = false;
            _accumulate = default;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (span.IsEmpty)
                return true;

            var idx = 0;
            var accumulate = _accumulate;
            if (!_hasData)
            {
                _hasData = true;
                accumulate = span[0];
                idx = 1;
            }

            var func = _func;
            for (; idx < span.Length; ++idx)
                accumulate = func(accumulate, span[idx]);

            _accumulate = accumulate;

            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        internal T GetResult()
        {
            if (!_hasData)
                throw new InvalidOperationException();

            return _accumulate;
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_hasData)
            {
                _accumulate = _func(_accumulate, input);
            }
            else
            {
                _hasData = true;
                _accumulate = input;
            }
            return true;
        }
    }
}
