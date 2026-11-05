using Cistern.ValueLinq.Maths;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    partial struct Average<T, TAccumulator, TQuotient>
        : IPushEnumerator<T>
        where T : struct, INumber<T>
        where TAccumulator : struct, INumber<TAccumulator>
        where TQuotient : struct, INumber<TQuotient>
    {
        private SIMDOptions _options;
        private TAccumulator sum;
        private long counter;

        public Average(SIMDOptions options)
        {
            sum = TAccumulator.Zero;
            counter = 0;
            _options = options;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch) => ProcessSpan(batch.GetSpan());

        private bool ProcessSpan(ReadOnlySpan<T> span)
            => SIMD.Average(span, _options, ref sum, ref counter);

        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public TQuotient GetResult()
        {
            if (counter == 0)
            {
                ThrowHelper.ThrowNoElementsException();
            }

            return Number.Divide<TAccumulator, TQuotient>(sum, counter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            sum = checked(sum + TAccumulator.CreateChecked(input));
            ++counter;
            return true;
        }
    }

    [Aggregator]
    partial struct AverageNullable<T, TAccumulator, TQuotient>
        : IPushEnumerator<T?>
        where T : struct, INumber<T>
        where TAccumulator : struct, INumber<TAccumulator>
        where TQuotient : struct, INumber<TQuotient>
    {
        private TAccumulator sum;
        private long counter;

        public AverageNullable(bool _)
        {
            sum = TAccumulator.Zero;
            counter = 0;
        }

        public TPushResult GetResult<TPushResult>() => Impl.ResultCast<TQuotient?, TPushResult>(GetResult());

        internal TQuotient? GetResult() =>
            counter == 0
                ? null
                : Number.Divide<TAccumulator, TQuotient>(sum, counter);

        public bool ProcessNext(T? input)
        {
            if (input.HasValue)
            {
                sum = checked(sum + TAccumulator.CreateChecked(input.GetValueOrDefault()));
                ++counter;
            }

            return true;
        }
    }
}
