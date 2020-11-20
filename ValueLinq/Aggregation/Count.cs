using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Count<T>
        : IForwardEnumerator<T>
    {
        private int _count;

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_count;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            checked
            {
                ++_count;
                return true;
            }
        }
    }

    struct CountIf<T>
        : IForwardEnumerator<T>
    {
        private Func<T, bool> _predicate;
        private int _count;

        public CountIf(Func<T, bool> predicate) => (_predicate, _count) = (predicate, 0);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public int GetResult() => _count;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (_predicate(input))
            {
                ++_count;
            }
            return true;
        }
    }


}
