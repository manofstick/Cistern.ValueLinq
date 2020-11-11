using Cistern.ValueLinq.Maths;

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
