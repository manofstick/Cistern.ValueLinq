using Cistern.ValueLinq.Maths;
using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    struct Average<T, Accumulator, Quotient, Math>
        : IPushEnumerator<T>
        where T : struct
        where Accumulator : struct
        where Quotient : struct
        where Math : struct, IMathsOperations<T, Accumulator, Quotient>
    {
        static Math math = default;

        private SIMDOptions _options;
        private Accumulator sum;
        private long counter;

        public Average(SIMDOptions options) => (sum, counter, _options) = (math.Zero, 0, options);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request)
        {
            if (typeof(TRequest) == typeof(Containers.GetSpan<TObject, T>))
            {
                var getSpan = (Containers.GetSpan<TObject, T>)(object)request;
                return ProcessBatch(getSpan(obj));
            }
            return BatchProcessResult.Unavailable;
        }

        private BatchProcessResult ProcessBatch(ReadOnlySpan<T> span)
            => SIMD.Average<T, Accumulator, Quotient, Math>(span, _options, ref sum, ref counter);

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
        : IPushEnumerator<T?>
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
