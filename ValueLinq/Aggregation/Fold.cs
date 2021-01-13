using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct FoldForward<T, TAccumulate>
        : IPushEnumerator<T>
    {
        private TAccumulate _accumulate;
        private Func<TAccumulate, T, TAccumulate> _func;

        public FoldForward(Func<TAccumulate, T, TAccumulate> func, TAccumulate seed) => (_func, _accumulate) = (func, seed);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IPushEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public TAccumulate GetResult() => _accumulate;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _accumulate = _func(_accumulate, input);
            return true;
        }
    }
}
