using Cistern.ValueLinq.Maths;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

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

        private T result;
        private bool noData;

        public Max(bool _) => (noData, result) = (true, math.MaxInit);

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
            if (math.IsNaN(result) || math.GreaterThan(input, result))
                result = input;
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
