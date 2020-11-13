using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct ReduceForward<T>
        : IForwardEnumerator<T>
    {
        private bool _hasData;
        private T _accumulate;
        private Func<T, T, T> _func;

        public ReduceForward(Func<T, T, T> func) => (_func, _hasData, _accumulate) = (func, false, default);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>()
        {
            if (!_hasData)
                throw new InvalidOperationException();

            return (TResult)(object)_accumulate;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
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
