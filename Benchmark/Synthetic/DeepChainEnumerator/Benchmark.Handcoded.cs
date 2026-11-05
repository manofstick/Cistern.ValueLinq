using BenchmarkDotNet.Attributes;
using System;

namespace Cistern.Benchmarks.Synthetic
{
    partial class DeepChainEnumerator
    {
        [BenchmarkCategory("Depth4"), Benchmark(Baseline = true)]
        public long Handcoded_Depth4() => SumOffset(_array, 2);

        [BenchmarkCategory("Depth16"), Benchmark(Baseline = true)]
        public long Handcoded_Depth16() => SumOffset(_array, 8);

        [BenchmarkCategory("Depth32"), Benchmark(Baseline = true)]
        public long Handcoded_Depth32() => SumOffset(_array, 16);

        [BenchmarkCategory("Concat"), Benchmark(Baseline = true)]
        public long Handcoded_Concat() => 2 * SumOffset(_array, 9);

        private static long SumOffset(ReadOnlySpan<int> source, int offset)
        {
            var sum = 0L;
            foreach (var x in source)
                sum += x + offset;
            return sum;
        }
    }
}
