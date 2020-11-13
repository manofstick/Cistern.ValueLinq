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
        {
            var sum = this.sum;
            foreach (var x in span)
                sum = math.Add(sum, x);
            this.sum = sum;

            return BatchProcessResult.SuccessAndContinue;
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

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
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
