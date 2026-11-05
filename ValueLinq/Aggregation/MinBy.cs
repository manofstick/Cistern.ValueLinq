using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    // a value-type key under the default comparer is compared through Comparer<TKey>.Default called directly, which the
    // JIT devirtualizes and inlines, as System.Linq's MinBy does
    [Aggregator]
    partial struct MinBy<T, TKey>
        : IPushEnumerator<T>
    {
        private readonly Func<T, TKey> _keySelector;
        private readonly IComparer<TKey> _comparer;
        private readonly bool _defaultComparer;
        private T? _result;
        private TKey? _key;
        private bool _noData;

        public MinBy(Func<T, TKey> keySelector, IComparer<TKey>? comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            _keySelector = keySelector;
            _comparer = comparer ?? Comparer<TKey>.Default;
            _defaultComparer = default(TKey) != null && ReferenceEquals(_comparer, Comparer<TKey>.Default);
            _result = default;
            _key = default;
            _noData = true;
        }

        // the span runs from locals: Docs/BatchFromLocalCopy.md, "The fix"
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (span.IsEmpty)
                return true;

            var keySelector = _keySelector;
            var comparer = _comparer;
            var result = _result;
            var key = _key;
            var start = 0;

            if (_noData)
            {
                result = span[0];
                key = keySelector(result);
                start = 1;
            }

            if (_defaultComparer)
            {
                foreach (var input in span.Slice(start))
                {
                    var nextKey = keySelector(input);
                    if (Comparer<TKey>.Default.Compare(nextKey, key!) < 0)
                    {
                        result = input;
                        key = nextKey;
                    }
                }
            }
            else
            {
                foreach (var input in span.Slice(start))
                {
                    var nextKey = keySelector(input);
                    if (nextKey != null
                        && (key == null || comparer.Compare(nextKey, key) < 0))
                    {
                        result = input;
                        key = nextKey;
                    }
                }
            }

            _result = result;
            _key = key;
            _noData = false;

            return true;
        }

        public TPushResult GetResult<TPushResult>() => Impl.ResultCast<T?, TPushResult>(GetResult());

        public T? GetResult()
        {
            if (_noData
                && default(T) != null)
            {
                ThrowHelper.ThrowNoElementsException();
            }

            return _result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            var key = _keySelector(input);

            if (_noData)
            {
                _noData = false;
                _result = input;
                _key = key;
            }
            else if (_defaultComparer)
            {
                if (Comparer<TKey>.Default.Compare(key, _key!) < 0)
                {
                    _result = input;
                    _key = key;
                }
            }
            else if (key != null
                && (_key == null || _comparer.Compare(key, _key) < 0))
            {
                _result = input;
                _key = key;
            }

            return true;
        }
    }
}
