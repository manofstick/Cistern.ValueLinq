using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Single<T>
        : IForwardEnumerator<T>
    {
        private T _single;
        private int _count;

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult()
        {
            if (_count != 1)
                throw new InvalidOperationException();
            return _single;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (_count == 0)
            {
                _count = 1;
                _single = input;
                return true;
            }
            _count = 2;
            return false;
        }
    }

    struct SingleOrDefault<T>
        : IForwardEnumerator<T>
    {
        private T _single;
        private int _count;

        public SingleOrDefault(bool _) => (_single, _count) = (default, 0);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();
        public T GetResult()
        {
            if (_count > 1)
                throw new InvalidOperationException();
            return _single;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (_count == 0)
            {
                _count = 1;
                _single = input;
                return true;
            }
            _count = 2;
            return false;
        }
    }

    struct SinglePredicate<T>
        : IForwardEnumerator<T>
    {
        private T _single;
        private int _count;
        Func<T, bool> _predicate;

        public SinglePredicate(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            (_predicate, _single, _count) = (predicate, default, 0);
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult()
        {
            if (_count != 1)
                throw new InvalidOperationException();
            return _single;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate(input))
                return true;

            if (_count == 0)
            {
                _count = 1;
                _single = input;
                return true;
            }
            _count = 2;
            return false;
        }
    }

    struct SingleOrDefaultPredicate<T>
        : IForwardEnumerator<T>
    {
        private T _single;
        private int _count;
        Func<T, bool> _predicate;

        public SingleOrDefaultPredicate(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            (_predicate, _single, _count) = (predicate, default, 0);
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult()
        {
            if (_count > 1)
                throw new InvalidOperationException();
            return _single;
        }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate(input))
                return true;

            if (_count == 0)
            {
                _count = 1;
                _single = input;
                return true;
            }
            _count = 2;
            return false;
        }
    }
}
