using Cistern.ValueLinq.Maths;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Numerics;
using System.Runtime.InteropServices;

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
        {
            var result = _result;

            _noData &= source.Length == 0;

            var idx = 0;

            const int NumberOfVectorsToMakeThisWorthwhile = 5; // from some random testing
            if (Vector.IsHardwareAccelerated && math.SupportsVectorization && ((source.Length - idx) / Vector<T>.Count > NumberOfVectorsToMakeThisWorthwhile))
            {
                var asVector = MemoryMarshal.Cast<T, Vector<T>>(source);
                var mins = new Vector<T>(result);
                if (math.HasNaNs)
                {
                    var nan = new Vector<T>(math.NaN);
                    foreach (var v in asVector)
                    {
                        if (Vector.EqualsAny(Vector.Xor(v, nan), Vector<T>.Zero))
                        {
                            _result = math.NaN;
                            return BatchProcessResult.SuccessAndHalt;
                        }
                        mins = Vector.Min(mins, v);
                    }
                }
                else
                {
                    foreach (var v in asVector)
                    {
                        mins = Vector.Min(mins, v);
                    }
                }

                for (var i = 0; i < Vector<T>.Count; ++i)
                {
                    var input = mins[i];
                    if (math.LessThan(input, result))
                        result = input;
                }

                idx += asVector.Length * Vector<T>.Count;
            }

            for (; idx < source.Length; ++idx)
            {
                var input = source[idx];
                if (math.LessThan(input, result))
                    result = input;
                else if (math.IsNaN(input))
                {
                    result = input;
                    break;
                }
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

        public TResult GetResult<TResult>() =>
            noData ? default : (TResult)(object)result;

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
