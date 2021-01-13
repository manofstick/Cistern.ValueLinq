using System;
using System.Collections.Generic;

#nullable enable

namespace Cistern.ValueLinq.Aggregation
{
    struct Contains<T>
        : IPushEnumerator<T>
    {
        private T _value;
        private bool _contains;

        public Contains(T value) => (_value, _contains) = (value, false);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IPushEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();
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

    struct ContainsByComparer<T>
        : IPushEnumerator<T>
    {
        private IEqualityComparer<T> _comparer;
        private T _value;
        private bool _contains;

        public ContainsByComparer(IEqualityComparer<T> comparer, T value) => (_comparer, _value, _contains) = (comparer ?? EqualityComparer<T>.Default, value, false);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IPushEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();
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
