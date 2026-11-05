using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct SequenceEqual<T>
        : IPushEnumerator<T>
    {
        private readonly PullEnumerator<T> _second;
        private readonly IEqualityComparer<T> _comparer;
        private bool _mismatch;

        public SequenceEqual(PullEnumerator<T> second, IEqualityComparer<T>? comparer)
        {
            _second = second;
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _mismatch = false;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            foreach (var input in span)
            {
                if (!ProcessNext(input))
                    return false;
            }

            return true;
        }

        public TPushResult GetResult<TPushResult>() => Impl.ResultCast<bool, TPushResult>(GetResult());

        public bool GetResult()
        {
            var equal =
                !_mismatch
                && !_second.TryGetNext(out _);

            return equal;
        }

        public bool ProcessNext(T input)
        {
            _mismatch =
                !_second.TryGetNext(out var other)
                || !_comparer.Equals(input, other);

            return !_mismatch;
        }
    }
}
