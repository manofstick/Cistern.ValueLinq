using Cistern.ValueLinq.Maths;
using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Sum<T, Accumulator, Quotient, Math>
        : IForwardEnumerator<T>
        where T : struct
        where Accumulator : struct
        where Quotient : struct
        where Math : struct, IMathsOperations<T, Accumulator, Quotient>
    {
        static Math math = default;

        private Accumulator sum;

        public Sum(bool _) => (sum) = (math.Zero);

        public bool CheckForOptimization<TObject, TRequest, TResult>(TObject obj, in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Containers.GetSpan<TObject, T>))
            {
                var getSpan = (Containers.GetSpan<TObject, T>)(object)request;
                result = (TResult)(object)GetSum(getSpan(obj));
                return true;
            }

            result = default;
            return false;
        }

        private T GetSum(ReadOnlySpan<T> span)
        {
            var sum = math.Zero;
            foreach (var x in span)
                sum = math.Add(sum, x);
            return math.Cast(sum);
        }

        public void Dispose() { }
        public TResult GetResult<TResult>() => (TResult)(object)math.Cast(sum);

        public bool ProcessNext(T input)
        {
            sum = math.Add(sum, input);
            return true;
        }
    }

    struct SumNullable<T, Accumulator, Quotient, Math>
        : IForwardEnumerator<T?>
        where T : struct
        where Accumulator : struct
        where Quotient : struct
        where Math : struct, IMathsOperations<T, Accumulator, Quotient>
    {
        static Math math = default;

        private Accumulator sum;

        public bool CheckForOptimization<TObject, TRequest, TResult>(TObject obj, in TRequest request, out TResult result) { result = default; return false; }
        public void Dispose() { }
        public SumNullable(bool _) => sum = math.Zero;

        public TResult GetResult<TResult>() => (TResult)(object)math.Cast(sum);

        public bool ProcessNext(T? input)
        {
            sum = math.Add(sum, input);
            return true;
        }
    }

}
