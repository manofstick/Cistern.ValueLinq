using BenchmarkDotNet.Attributes;
using System;

namespace Cistern.Benchmarks.Synthetic
{
    partial class DeepChainSum
    {
        [BenchmarkCategory("Depth1"), Benchmark(Baseline = true)]
        public int Handcoded_Depth1() => SumOffset(_array, 1);

        [BenchmarkCategory("Depth2"), Benchmark(Baseline = true)]
        public int Handcoded_Depth2() => SumOffset(_array, 1);

        [BenchmarkCategory("Depth4"), Benchmark(Baseline = true)]
        public int Handcoded_Depth4() => SumOffset(_array, 2);

        [BenchmarkCategory("Depth16"), Benchmark(Baseline = true)]
        public int Handcoded_Depth16() => SumOffset(_array, 8);

        [BenchmarkCategory("Depth32"), Benchmark(Baseline = true)]
        public int Handcoded_Depth32() => SumOffset(_array, 16);

        [BenchmarkCategory("Concat"), Benchmark(Baseline = true)]
        public int Handcoded_Concat() => 2 * SumOffset(_array, 9);

        private static int SumOffset(ReadOnlySpan<int> source, int offset)
        {
            var sum = 0;
            foreach (var x in source)
                sum += x + offset;
            return sum;
        }
    }
}
