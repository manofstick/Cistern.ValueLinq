using System;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct All<T>
        : IPushEnumerator<T>
    {
        private Func<T, bool> _predicate;
        private bool _all;

        public All(Func<T, bool> predicate)
        {
            _predicate = predicate;
            _all = true;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var predicate = _predicate;
            foreach (var item in span)
            {
                if (!predicate(item))
                {
                    _all = false;
                    return false;
                }
            }
            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public bool GetResult() => _all;
        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate(input))
            {
                _all = false;
                return false;
            }
            return true;
        }
    }

    [Aggregator]
    partial struct All<T, Func>
        : IPushEnumerator<T>
        where Func : IFunc<T, bool>
    {
        private Func _predicate;
        private bool _all;

        public All(Func predicate)
        {
            _predicate = predicate;
            _all = true;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            // the predicate is a struct that may carry state, so the copy is written back
            var predicate = _predicate;
            var more = true;
            foreach (var item in span)
            {
                if (!predicate.Invoke(item))
                {
                    _all = false;
                    more = false;
                    break;
                }
            }
            _predicate = predicate;
            return more;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public bool GetResult() => _all;
        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate.Invoke(input))
            {
                _all = false;
                return false;
            }
            return true;
        }
    }
}
