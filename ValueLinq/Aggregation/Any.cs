using System;

#nullable enable

namespace Cistern.ValueLinq.Aggregation
{
    struct Any<T>
        : IForwardEnumerator<T>
    {
        private Func<T, bool> _predicate;
        private bool _any;

        public Any(Func<T, bool> predicate) => (_predicate, _any) = (predicate, false);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();
        public bool GetResult() => _any;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (_predicate(input))
            {
                _any = true;
                return false;
            }
            return true;
        }
    }

    struct Anything<T>
        : IForwardEnumerator<T>
    {
        private bool _any;

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_any;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _any = true;
            return false;
        }
    }
}
