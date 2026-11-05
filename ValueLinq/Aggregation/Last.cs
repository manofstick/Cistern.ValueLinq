using System;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct Last<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _last;
        private bool _found;

        public Last() { }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        public T GetResult()
        {
            if (!_found)
                throw new InvalidOperationException();
            return _last;
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _found = true;
            _last = input;
            return true;
        }
    }

    [Aggregator]
    partial struct LastOrDefault<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _last;

        public LastOrDefault(T defaultValue) => _last = defaultValue;

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (!span.IsEmpty)
                _last = span[^1];

            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());
        public T GetResult() => _last;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _last = input;
            return true;
        }
    }

    [Aggregator]
    partial struct LastPredicate<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _last;
        private bool _found;
        Func<T, bool> _predicate;

        public LastPredicate(Func<T, bool> predicate)
        {
            _predicate = predicate;
            _last = default;
            _found = false;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        public T GetResult()
        {
            if (!_found)
                throw new InvalidOperationException();
            return _last;
        }

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_predicate(input))
            {
                _found = true;
                _last = input;
            }
            return true;
        }
    }

    [Aggregator]
    partial struct LastOrDefaultPredicate<T>
        : IPushEnumerator<T>
    {
        [AllowNull] private T _last;
        Func<T, bool> _predicate;

        // a null predicate must throw even over an empty source, so it is checked here rather than on first use
        public LastOrDefaultPredicate(Func<T, bool> predicate, [AllowNull] T defaultValue = default)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _last = defaultValue;
        }

        // the predicate runs on every element front to back, so it is not searched from the span's end
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var predicate = _predicate;
            foreach (var item in span)
            {
                if (predicate(item))
                    _last = item;
            }

            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());

        public T GetResult() => _last;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_predicate(input))
            {
                _last = input;
            }
            return true;
        }
    }
}
