﻿using Cistern.ValueLinq.Maths;

namespace Cistern.ValueLinq.Aggregation
{
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

        public TResult GetResult<TResult>()
        {
            if (noData)
                ThrowHelper.ThrowNoElementsException();
            return (TResult)(object)result;
        }

        public void Init(int? size)
        {
            noData = true;
            result = math.MaxInit;
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

        public TResult GetResult<TResult>() =>
            noData ? default : (TResult)(object)result;

        public void Init(int? size)
        {
            noData = true;
            result = math.MaxInit;
        }

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