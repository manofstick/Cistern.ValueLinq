using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct First<T>
        : IForwardEnumerator<T>
    {
        private T _first;
        private bool _found;

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult()
        {
            if (!_found)
                throw new InvalidOperationException();
            return _first;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _found = true;
            _first = input;
            return false;
        }
    }

    struct FirstOrDefault<T>
        : IForwardEnumerator<T>
    {
        private T _First;

        public FirstOrDefault(bool _) => _First = default;

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();
        public T GetResult() => _First;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _First = input;
            return false;
        }
    }

    struct FirstPredicate<T>
        : IForwardEnumerator<T>
    {
        private T _first;
        private bool _found;
        Func<T, bool> _predicate;

        public FirstPredicate(Func<T, bool> predicate) => (_predicate, _first, _found) = (predicate, default, false);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult()
        {
            if (!_found)
                throw new InvalidOperationException();
            return _first;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate(input))
                return true;

            _found = true;
            _first = input;
            return false;
        }
    }

    struct FirstOrDefaultPredicate<T>
        : IForwardEnumerator<T>
    {
        private T _first;
        Func<T, bool> _predicate;

        public FirstOrDefaultPredicate(Func<T, bool> predicate) => (_predicate, _first) = (predicate, default);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult() => _first;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate(input))
                return true;

            _first = input;
            return false;
        }
    }
}
