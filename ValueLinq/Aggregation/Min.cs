using Cistern.ValueLinq.Maths;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

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

        private T result;
        private bool noData;

        public Min(bool _) => (noData, result) = (true, math.MinInit);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        public TResult GetResult<TResult>()
        {
            if (noData)
                ThrowHelper.ThrowNoElementsException();
            return (TResult)(object)result;
        }

        public bool ProcessNext(T input)
        {
            noData = false;
            if (math.IsNaN(input))
            {
                result = math.NaN;
                return false;
            }
            if (math.LessThan(input, result))
                result = input;
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
