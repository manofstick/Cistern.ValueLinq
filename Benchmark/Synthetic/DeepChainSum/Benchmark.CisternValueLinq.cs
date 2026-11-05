using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;

namespace Cistern.Benchmarks.Synthetic
{
    partial class DeepChainSum
    {
        private static Func<int> BuildCisternValueLinq_Depth1(int[] source)
        {
            var chain =
                source
                .Select(x => x + 1);

            return () => chain.Sum();
        }

        private static Func<int> BuildCisternValueLinq_Depth2(int[] source)
        {
            var chain =
                source
                .Select(x => x + 1).Where(x => x >= 0);

            return () => chain.Sum();
        }

        private static Func<int> BuildCisternValueLinq_Depth4(int[] source)
        {
            var chain =
                source
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0);

            return () => chain.Sum();
        }

        private static Func<int> BuildCisternValueLinq_Depth16(int[] source)
        {
            var chain =
                source
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0);

            return () => chain.Sum();
        }

        private static Func<int> BuildCisternValueLinq_Depth32(int[] source)
        {
            var chain =
                source
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0);

            return () => chain.Sum();
        }

        // each part of a Concat is pushed into the downstream chain the Concat was handed
        private static Func<int> BuildCisternValueLinq_Concat(int[] source)
        {
            var start =
                source
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0);

            var finish =
                source
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0);

            var chain =
                start
                .Concat(finish)
                .Select(x => x + 1).Where(x => x >= 0);

            return () => chain.Sum();
        }

        [BenchmarkCategory("Depth1"), Benchmark]
        public int CisternValueLinq_Depth1() => _depth1();

        [BenchmarkCategory("Depth2"), Benchmark]
        public int CisternValueLinq_Depth2() => _depth2();

        [BenchmarkCategory("Depth4"), Benchmark]
        public int CisternValueLinq_Depth4() => _depth4();

        [BenchmarkCategory("Depth16"), Benchmark]
        public int CisternValueLinq_Depth16() => _depth16();

        [BenchmarkCategory("Depth32"), Benchmark]
        public int CisternValueLinq_Depth32() => _depth32();

        [BenchmarkCategory("Concat"), Benchmark]
        public int CisternValueLinq_Concat() => _concat();

        // built in each call, as a ZLinq chain has to be
        [BenchmarkCategory("Depth1"), Benchmark]
        public int CisternValueLinqPerCall_Depth1() =>
            _array
            .Select(x => x + 1)
            .Sum();

        [BenchmarkCategory("Depth2"), Benchmark]
        public int CisternValueLinqPerCall_Depth2() =>
            _array
            .Select(x => x + 1).Where(x => x >= 0)
            .Sum();

        [BenchmarkCategory("Depth4"), Benchmark]
        public int CisternValueLinqPerCall_Depth4() =>
            _array
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Sum();

        [BenchmarkCategory("Depth16"), Benchmark]
        public int CisternValueLinqPerCall_Depth16() =>
            _array
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Sum();

        [BenchmarkCategory("Depth32"), Benchmark]
        public int CisternValueLinqPerCall_Depth32() =>
            _array
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Sum();

        [BenchmarkCategory("Concat"), Benchmark]
        public int CisternValueLinqPerCall_Concat() =>
            _array
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Concat(
                _array
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0))
            .Select(x => x + 1).Where(x => x >= 0)
            .Sum();
    }
}
