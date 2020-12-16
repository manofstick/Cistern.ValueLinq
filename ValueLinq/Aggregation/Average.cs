using Cistern.ValueLinq.Maths;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    struct Average<T, Accumulator, Quotient, Math>
        : IForwardEnumerator<T>
        where T : struct
        where Accumulator : struct
        where Quotient : struct
        where Math : struct, IMathsOperations<T, Accumulator, Quotient>
    {
        static Math math = default;

        private Accumulator sum;
        private long counter;

        public Average(bool _) => (sum, counter) = (math.Zero, 0);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        public TResult GetResult<TResult>() => (TResult)(object)GetResult();

        public Quotient GetResult()
        {
            if (counter == 0)
            {
                ThrowHelper.ThrowNoElementsException();
            }

            return math.DivLong(sum, counter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            sum = math.Add(sum, input);
            ++counter;
            return true;
        }
    }

    struct AverageNullable<T, Accumulator, Quotient, Math>
        : IForwardEnumerator<T?>
        where T : struct
        where Accumulator : struct
        where Quotient : struct
        where Math : struct, IMathsOperations<T, Accumulator, Quotient>
    {
        static Math math = default;

        private Accumulator sum;
        private long counter;

        public AverageNullable(bool _) => (sum, counter) = (math.Zero, 0);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        public TResult GetResult<TResult>() => (TResult)(object)GetResult();

        internal Quotient? GetResult() =>
            counter == 0 
                ? null
                : math.DivLong(sum, counter);

        public bool ProcessNext(T? input)
        {
            if (input.HasValue)
            {
                sum = math.Add(sum, input);
                ++counter;
            }

            return true;
        }
    }
}
