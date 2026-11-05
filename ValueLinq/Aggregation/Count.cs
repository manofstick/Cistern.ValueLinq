using System;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct Count<T>
        : IPushEnumerator<T>
    {
        private int _count;

        public Count() { }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)_count;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            checked
            {
                ++_count;
                return true;
            }
        }
    }

    [Aggregator]
    partial struct CountIf<T>
        : IPushEnumerator<T>
    {
        private Func<T, bool> _predicate;
        private int _count;

        public CountIf(Func<T, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _count = 0;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var predicate = _predicate;
            var count = _count;
            foreach (var item in span)
            {
                if (predicate(item))
                    ++count;
            }
            _count = count;
            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public int GetResult() => _count;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_predicate(input))
            {
                ++_count;
            }
            return true;
        }
    }

    [Aggregator]
    partial struct LongCount<T>
        : IPushEnumerator<T>
    {
        private long _count;

        public LongCount() { }

        // only the length is read, so a cold batch is never copied out
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdCopy;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            checked
            {
                _count += batch.Count;
            }

            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)_count;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            checked
            {
                ++_count;
                return true;
            }
        }
    }

    [Aggregator]
    partial struct LongCountIf<T>
        : IPushEnumerator<T>
    {
        private Func<T, bool> _predicate;
        private long _count;

        public LongCountIf(Func<T, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _count = 0;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var predicate = _predicate;
            var count = _count;
            foreach (var item in span)
            {
                if (predicate(item))
                    checked { ++count; }
            }
            _count = count;

            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public long GetResult() => _count;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_predicate(input))
            {
                checked { ++_count; }
            }

            return true;
        }
    }
}
