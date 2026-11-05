using System;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct Single<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _single;
        private int _count;

        public Single() { }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        public T GetResult()
        {
            if (_count != 1)
                throw new InvalidOperationException();
            return _single;
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
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

    [Aggregator]
    partial struct SingleOrDefault<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _single;
        private int _count;

        public SingleOrDefault(T defaultValue)
        {
            _single = defaultValue;
            _count = 0;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (span.IsEmpty)
                return true;

            if (_count == 0 && span.Length == 1)
            {
                _count = 1;
                _single = span[0];
                return true;
            }

            _count = 2;
            return false;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());
        public T GetResult()
        {
            if (_count > 1)
                throw new InvalidOperationException();
            return _single;
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
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

    [Aggregator]
    partial struct SinglePredicate<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _single;
        private int _count;
        Func<T, bool> _predicate;

        public SinglePredicate(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            _predicate = predicate;
            _single = default;
            _count = 0;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var predicate = _predicate;
            foreach (var item in span)
            {
                if (!predicate(item))
                    continue;

                if (_count != 0)
                {
                    _count = 2;
                    return false;
                }

                _count = 1;
                _single = item;
            }

            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        public T GetResult()
        {
            if (_count != 1)
                throw new InvalidOperationException();
            return _single;
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
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

    [Aggregator]
    partial struct SingleOrDefaultPredicate<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _single;
        private int _count;
        Func<T, bool> _predicate;

        public SingleOrDefaultPredicate(Func<T, bool> predicate, [AllowNull] T defaultValue = default)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            _predicate = predicate;
            _single = defaultValue;
            _count = 0;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var predicate = _predicate;
            foreach (var item in span)
            {
                if (!predicate(item))
                    continue;

                if (_count != 0)
                {
                    _count = 2;
                    return false;
                }

                _count = 1;
                _single = item;
            }

            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        public T GetResult()
        {
            if (_count > 1)
                throw new InvalidOperationException();
            return _single;
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
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
