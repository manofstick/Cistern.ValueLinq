using Cistern.ValueLinq.Maths;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct Min<T>
        : IPushEnumerator<T>
    {
        private static IComparer<T> comparer =
            typeof(T) == typeof(string)
                ? (IComparer<T>)StringComparer.CurrentCulture
                : Comparer<T>.Default;

        private T? result;
        private bool noData;

        public Min(bool _)
        {
            noData = true;
            result = default;
        }

        // a reference type with no SIMD path is left to the element loop
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen =>
            SIMD.HasExtremum<T>() || default(T) != null
                ? BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan
                : BatchKinds.None;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (SIMD.HasExtremum<T>())
                return SIMD.Extremum(span, least: true, ref result!, ref noData);

            return DefaultComparerExtremum.Min(span, ref result!, ref noData);
        }

        public TPushResult GetResult<TPushResult>() => Impl.ResultCast<T?, TPushResult>(GetResult());

        public T? GetResult()
        {
            if (noData && default(T) != null)
                ThrowHelper.ThrowNoElementsException();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            if (noData)
            {
                noData = false;
                result = input;
            }
            else if (default(T) != null)
            {
                if (Comparer<T>.Default.Compare(input, result!) < 0)
                    result = input;
            }
            else
            {
                if (result == null || (input != null && comparer.Compare(input, result) < 0))
                    result = input;
            }
            return true;
        }
    }

    // a value type compared through Comparer<T>.Default called directly, which the JIT devirtualizes and inlines, as
    // System.Linq compares one; the span runs from locals: Docs/BatchFromLocalCopy.md, "The fix"
    static class DefaultComparerExtremum
    {
        internal static bool Min<T>(ReadOnlySpan<T> span, ref T result, ref bool noData)
        {
            if (span.IsEmpty)
                return true;

            var local = result;
            var i = 0;
            if (noData)
            {
                noData = false;
                local = span[0];
                i = 1;
            }

            for (; i < span.Length; ++i)
            {
                if (Comparer<T>.Default.Compare(span[i], local) < 0)
                    local = span[i];
            }

            result = local;

            return true;
        }

        internal static bool Max<T>(ReadOnlySpan<T> span, ref T result, ref bool noData)
        {
            if (span.IsEmpty)
                return true;

            var local = result;
            var i = 0;
            if (noData)
            {
                noData = false;
                local = span[0];
                i = 1;
            }

            for (; i < span.Length; ++i)
            {
                if (Comparer<T>.Default.Compare(span[i], local) > 0)
                    local = span[i];
            }

            result = local;

            return true;
        }
    }

    // float and double stay off SIMD.Min: Vector.Min returns -0.0 for a -0.0 and a +0.0, where Comparer<T>.Default
    // ties them and keeps the first, as System.Linq's comparer form does
    [Aggregator]
    partial struct MinComparer<T>
        : IPushEnumerator<T>
    {
        private readonly IComparer<T> _comparer;
        private T? _result;
        private bool _noData;

        public MinComparer(IComparer<T>? comparer)
        {
            _comparer = comparer ?? Comparer<T>.Default;
            _result = default;
            _noData = true;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;

        // the span runs from locals: Docs/BatchFromLocalCopy.md, "The fix"
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            if (SIMD.HasExtremum<T>() && ReferenceEquals(_comparer, Comparer<T>.Default))
                return SIMD.Extremum(span, least: true, ref _result!, ref _noData);

            if (span.IsEmpty)
                return true;

            var comparer = _comparer;
            var result = _result;
            var i = 0;

            if (_noData)
            {
                _noData = false;
                result = span[0];
                i = 1;
            }

            for (; i < span.Length; ++i)
            {
                var input = span[i];
                if (result == null || (input != null && comparer.Compare(input, result) < 0))
                    result = input;
            }

            _result = result;

            return true;
        }

        public TPushResult GetResult<TPushResult>() => Impl.ResultCast<T?, TPushResult>(GetResult());

        public T? GetResult()
        {
            if (_noData && default(T) != null)
                ThrowHelper.ThrowNoElementsException();

            return _result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            if (_noData)
            {
                _noData = false;
                _result = input;
            }
            else if (_result == null || (input != null && _comparer.Compare(input, _result) < 0))
            {
                _result = input;
            }

            return true;
        }
    }

    [Aggregator]
    partial struct MinNumber<T>
        : IPushEnumerator<T>
        where T : struct, INumber<T>
    {
        private T _result;
        private bool _noData;

        public MinNumber(bool _)
        {
            _noData = true;
            _result = default;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch) => ProcessSpan(batch.GetSpan());

        private bool ProcessSpan(ReadOnlySpan<T> source)
            => SIMD.Min(source, ref _result, ref _noData);

        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)GetResult();
        public T GetResult()
        {
            if (_noData)
                ThrowHelper.ThrowNoElementsException();
            return _result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            if (_noData || input < _result)
            {
                _noData = false;
                _result = input;
                return !T.IsNaN(input);
            }
            if (T.IsNaN(input))
            {
                _result = input;
                return false;
            }
            return true;
        }
    }

    [Aggregator]
    partial struct MinNumberNullable<T>
        : IPushEnumerator<T?>
        where T : struct, INumber<T>
    {
        private T result;
        private bool noData;

        public MinNumberNullable(bool _)
        {
            noData = true;
            result = default;
        }

        public TPushResult GetResult<TPushResult>() => Impl.ResultCast<T?, TPushResult>(GetResult());

        public T? GetResult() => noData ? (T?)null : result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T? input)
        {
            if (input.HasValue)
            {
                var value = input.GetValueOrDefault();
                if (noData || value < result)
                {
                    noData = false;
                    result = value;
                    return !T.IsNaN(value);
                }
                if (T.IsNaN(value))
                {
                    result = value;
                    return false;
                }
            }
            return true;
        }
    }

}
