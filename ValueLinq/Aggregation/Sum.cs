using Cistern.ValueLinq.Maths;
using System;
using System.Runtime.CompilerServices;

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

        private SIMDOptions _options;
        private Accumulator sum;

        public Sum(SIMDOptions options) => (_options, sum) = (options, math.Zero);

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
            => SIMD.Sum<T, Accumulator, Quotient, Math>(span, _options, ref sum);

        public void Dispose() { }
        public TResult GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult() => math.Cast(sum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public TResult GetResult<TResult>() => (TResult)(object)GetResult();

        public T GetResult() => math.Cast(sum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T? input)
        {
            sum = math.Add(sum, input);
            return true;
        }
    }

}
