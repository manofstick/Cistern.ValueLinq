using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Last<T>
        : IForwardEnumerator<T>
    {
        private T _last;
        private bool _found;

        public bool CheckForOptimization<TObject, TRequest, TResult>(TObject obj, in TRequest request, out TResult result) { result = default; return false; }
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>()
        {
            if (!_found)
                throw new InvalidOperationException();
            return (TResult)(object)_last;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _found = true;
            _last = input;
            return true;
        }
    }

    struct LastOrDefault<T>
        : IForwardEnumerator<T>
    {
        private T _last;

        public LastOrDefault(bool _) => _last = default;

        public bool CheckForOptimization<TObject, TRequest, TResult>(TObject obj, in TRequest request, out TResult result) { result = default; return false; }
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_last;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _last = input;
            return true;
        }
    }
}
