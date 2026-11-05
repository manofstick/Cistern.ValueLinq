using System;

#nullable enable

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct Any<T>
        : IPushEnumerator<T>
    {
        private Func<T, bool> _predicate;
        private bool _any;

        public Any(Func<T, bool> predicate)
        {
            _predicate = predicate;
            _any = false;
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
                    _any = true;
                    return false;
                }
            }
            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();
        public bool GetResult() => _any;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_predicate(input))
            {
                _any = true;
                return false;
            }
            return true;
        }
    }

    [Aggregator]
    partial struct Anything<T>
        : IPushEnumerator<T>
    {
        private bool _any;

        public Anything() { }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)_any;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _any = true;
            return false;
        }
    }
}
