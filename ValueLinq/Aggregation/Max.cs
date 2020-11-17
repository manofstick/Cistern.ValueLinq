using Cistern.ValueLinq.Maths;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cistern.ValueLinq.Aggregation
{
    struct Max<T>
        : IForwardEnumerator<T>
    {
        private static IComparer<T> comparer = 
            typeof(T) == typeof(string)
                ? (IComparer<T>)StringComparer.CurrentCulture
                : Comparer<T>.Default;

        private T result;
        private bool noData;

        public Max(bool _) => (noData, result) = (true, default);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        public TResult GetResult<TResult>()
        {
            if (noData && default(TResult) != null)
                ThrowHelper.ThrowNoElementsException();
            return (TResult)(object)result;
        }

        public bool ProcessNext(T input)
        {
            if (noData)
            {
                noData = false;
                result = input;
            }
            else
            {
                if (comparer.Compare(input, result) > 0)
                    result = input;
            }
            return true;
        }
    }

    struct Max<T, Accumulator, Quotient, Math>
        : IForwardEnumerator<T>
        where T : struct
        where Accumulator : struct
        where Quotient : struct
        where Math : struct, IMathsOperations<T, Accumulator, Quotient>
    {
        static Math math = default;

        private T _result;
        private bool _noData;

        public Max(bool _) => (_noData, _result) = (true, math.MaxInit);

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
        {
            var result = _result;

            _noData &= source.Length == 0;
            var idx = 0;
            for (; math.IsNaN(result) && idx < source.Length; ++idx)
            {
                result = source[idx];
            }

            const int NumberOfVectorsToMakeThisWorthwhile = 5; // from some random testing
            if (Vector.IsHardwareAccelerated && math.SupportsVectorization && ((source.Length - idx) / Vector<T>.Count > NumberOfVectorsToMakeThisWorthwhile))
            {
                var remainder = source.Slice(idx);
                var asVector = MemoryMarshal.Cast<T, Vector<T>>(remainder);
                var maxes = new Vector<T>(result);
                foreach (var v in asVector)
                {
                    maxes = Vector.Max(maxes, v);
                }
                for (var i = 0; i < Vector<T>.Count; ++i)
                {
                    var input = maxes[i];
                    if (math.GreaterThan(input, result))
                        result = input;
                }

                idx += asVector.Length * Vector<T>.Count;
            }

            for (; idx < source.Length; ++idx)
            {
                var input = source[idx];
                if (math.GreaterThan(input, result))
                    result = input;
            }

            _result = result;

            return BatchProcessResult.SuccessAndContinue;
        }

        public void Dispose() { }
        public TResult GetResult<TResult>()
        {
            if (_noData)
                ThrowHelper.ThrowNoElementsException();
            return (TResult)(object)_result;
        }

        public bool ProcessNext(T input)
        {
            _noData = false;
            if (math.IsNaN(_result) || math.GreaterThan(input, _result))
                _result = input;
            return true;
        }
    }

    struct MaxNullable<T, Accumulator, Quotient, Math>
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
        public MaxNullable(bool _) => (noData, result) = (true, math.MaxInit);

        public TResult GetResult<TResult>() =>
            noData ? default : (TResult)(object)result;

        public bool ProcessNext(T? input)
        {
            if (input.HasValue)
            {
                noData = false;
                if (math.IsNaN(result) || math.GreaterThan(input.Value, result))
                    result = input.Value;
            }
            return true;
        }
    }

}
