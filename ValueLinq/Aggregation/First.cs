using System;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct First<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _first;
        private bool _found;

        public First() { }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        public T GetResult()
        {
            if (!_found)
                throw new InvalidOperationException();
            return _first;
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _found = true;
            _first = input;
            return false;
        }
    }

    [Aggregator]
    partial struct FirstOrDefault<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _first;

        public FirstOrDefault(T defaultValue) => _first = defaultValue;

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (span.IsEmpty)
                return true;

            _first = span[0];

            return false;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());
        public T GetResult() => _first;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _first = input;
            return false;
        }
    }

    [Aggregator]
    partial struct FirstPredicate<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _first;
        private bool _found;
        Func<T, bool> _predicate;

        public FirstPredicate(Func<T, bool> predicate)
        {
            _predicate = predicate;
            _first = default;
            _found = false;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var predicate = _predicate;
            foreach (var item in span)
            {
                if (predicate(item))
                {
                    _found = true;
                    _first = item;
                    return false;
                }
            }

            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        public T GetResult()
        {
            if (!_found)
                throw new InvalidOperationException();
            return _first;
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate(input))
                return true;

            _found = true;
            _first = input;
            return false;
        }
    }

    [Aggregator]
    partial struct FirstOrDefaultPredicate<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _first;
        Func<T, bool> _predicate;

        // a null predicate must throw even over an empty source, so it is checked here rather than on first use
        public FirstOrDefaultPredicate(Func<T, bool> predicate, [AllowNull] T defaultValue = default)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _first = defaultValue;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var predicate = _predicate;
            foreach (var item in span)
            {
                if (predicate(item))
                {
                    _first = item;
                    return false;
                }
            }

            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        public T GetResult() => _first;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate(input))
                return true;

            _first = input;
            return false;
        }
    }
}
