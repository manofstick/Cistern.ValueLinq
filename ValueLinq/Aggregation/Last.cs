using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Last<T>
        : IForwardEnumerator<T>
    {
        private T _last;
        private bool _found;

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult()
        {
            if (!_found)
                throw new InvalidOperationException();
            return _last;
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

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();
        public T GetResult() => _last;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _last = input;
            return true;
        }
    }

    struct LastPredicate<T>
        : IForwardEnumerator<T>
    {
        private T _last;
        private bool _found;
        Func<T, bool> _predicate;

        public LastPredicate(Func<T, bool> predicate) => (_predicate, _last, _found) = (predicate, default, false);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult()
        {
            if (!_found)
                throw new InvalidOperationException();
            return _last;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (_predicate(input))
            {
                _found = true;
                _last = input;
            }
            return true;
        }
    }

    struct LastOrDefaultPredicate<T>
        : IForwardEnumerator<T>
    {
        private T _last;
        Func<T, bool> _predicate;

        public LastOrDefaultPredicate(Func<T, bool> predicate) => (_predicate, _last) = (predicate, default);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult() => _last;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (_predicate(input))
            {
                _last = input;
            }
            return true;
        }
    }
}
