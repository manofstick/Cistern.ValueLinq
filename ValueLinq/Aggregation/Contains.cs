using System;
using System.Collections.Generic;

#nullable enable

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct Contains<T>
        : IPushEnumerator<T>
    {
        private T _value;
        private bool _contains;

        public Contains(T value)
        {
            _value = value;
            _contains = false;
        }

        // the span's own search, vectorised for a bitwise-equatable type, as System.Linq searches an array or list
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (span.IndexOf(_value, null) < 0)
                return true;

            _contains = true;
            return false;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();
        public bool GetResult() => _contains;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (EqualityComparer<T>.Default.Equals(_value, input))
            {
                _contains = true;
                return false;
            }
            return true;
        }
    }

    [Aggregator]
    partial struct ContainsByComparer<T>
        : IPushEnumerator<T>
    {
        private IEqualityComparer<T> _comparer;
        private T _value;
        private bool _contains;

        public ContainsByComparer(IEqualityComparer<T>? comparer, T value)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _value = value;
            _contains = false;
        }

        // the span's own search, which takes the vectorised path when the comparer is the default
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (span.IndexOf(_value, _comparer) < 0)
                return true;

            _contains = true;
            return false;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();
        public bool GetResult() => _contains;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (_comparer.Equals(_value, input))
            {
                _contains = true;
                return false;
            }
            return true;
        }
    }
}
