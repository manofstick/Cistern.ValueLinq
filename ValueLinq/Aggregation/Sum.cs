using Cistern.ValueLinq.Maths;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct Sum<T, TAccumulator>
        : IPushEnumerator<T>
        where T : struct, INumber<T>
        where TAccumulator : struct, INumber<TAccumulator>
    {
        private SIMDOptions _options;
        private TAccumulator sum;

        public Sum(SIMDOptions options)
        {
            _options = options;
            sum = TAccumulator.Zero;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch) => ProcessSpan(batch.GetSpan());

        private bool ProcessSpan(ReadOnlySpan<T> span)
            => SIMD.Sum(span, _options, ref sum);

        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public T GetResult() => T.CreateChecked(sum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            sum = checked(sum + TAccumulator.CreateChecked(input));
            return true;
        }
    }

    [Aggregator]
    partial struct SumNullable<T, TAccumulator>
        : IPushEnumerator<T?>
        where T : struct, INumber<T>
        where TAccumulator : struct, INumber<TAccumulator>
    {
        private TAccumulator sum;

        public SumNullable(bool _) => sum = TAccumulator.Zero;

        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public T GetResult() => T.CreateChecked(sum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T? input)
        {
            sum = checked(sum + TAccumulator.CreateChecked(input.GetValueOrDefault()));
            return true;
        }
    }

}
