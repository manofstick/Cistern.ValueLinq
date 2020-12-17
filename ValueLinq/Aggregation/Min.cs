using Cistern.ValueLinq.Maths;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    struct Min<T>
        : IForwardEnumerator<T>
    {
        private static IComparer<T> comparer =
            typeof(T) == typeof(string)
                ? (IComparer<T>)StringComparer.CurrentCulture
                : Comparer<T>.Default;

        private T result;
        private bool noData;

        public Min(bool _) => (noData, result) = (true, default);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        public TResult GetResult<TResult>() => (TResult)(object)GetResult();
        
        public T GetResult()
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
            else
            {
                if (result == null || (input != null && comparer.Compare(input, result) < 0))
                    result = input;
            }
            return true;
        }
    }

    struct Min<T, Accumulator, Quotient, Math>
        : IForwardEnumerator<T>
        where T : struct
        where Accumulator : struct
        where Quotient : struct
        where Math : struct, IMathsOperations<T, Accumulator, Quotient>
    {
        static Math math = default;

        private T _result;
        private bool _noData;

        public Min(bool _) => (_noData, _result) = (true, math.MinInit);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request)
        {
            if (typeof(TRequest) == typeof(Containers.GetSpan<TObject, T>))
            {
                var getSpan = (Containers.GetSpan<TObject, T>)(object)request;
                return ProcessBatch(getSpan(obj));
            }
            return BatchProcessResult.Unavailable;
        }

        private BatchProcessResult ProcessBatch(ReadOnlySpan<T> source)
            => SIMD.Min<T, Accumulator, Quotient, Math>(source, ref _result, ref _noData);

        public void Dispose() { }
        public TResult GetResult<TResult>() => (TResult)(object)GetResult();
        public T GetResult()
        {
            if (_noData)
                ThrowHelper.ThrowNoElementsException();
            return _result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            _noData = false;
            if (math.IsNaN(input))
            {
                _result = math.NaN;
                return false;
            }
            if (math.LessThan(input, _result))
                _result = input;
            return true;
        }
    }

    struct MinNullable<T, Accumulator, Quotient, Math>
        : IForwardEnumerator<T?>
        where T : struct
        where Accumulator : struct
        where Quotient : struct
        where Math : struct, IMathsOperations<T, Accumulator, Quotient>
    {
        static Math math = default;

        private T result;
        private bool noData;

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        public MinNullable(bool _) => (noData, result) = (true, math.MinInit);

        public TResult GetResult<TResult>() => (TResult)(object)GetResult();

        public T? GetResult() => noData ? (T?)null : result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T? input)
        {
            if (input.HasValue)
            {
                noData = false;
                if (math.IsNaN(input.Value))
                {
                    result = math.NaN;
                    return false;
                }
                if (math.LessThan(input.Value, result))
                    result = input.Value;
            }
            return true;
        }
    }

}
