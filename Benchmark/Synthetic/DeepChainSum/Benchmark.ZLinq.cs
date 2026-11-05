using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Synthetic
{
    // a ZLinq chain is a ref struct, so it cannot be held from setup: each call builds it
    partial class DeepChainSum
    {
        [BenchmarkCategory("Depth1"), Benchmark]
        public int ZLinq_Depth1() =>
            _array
            .AsValueEnumerable()
            .Select(x => x + 1)
            .Sum();

        [BenchmarkCategory("Depth2"), Benchmark]
        public int ZLinq_Depth2() =>
            _array
            .AsValueEnumerable()
            .Select(x => x + 1).Where(x => x >= 0)
            .Sum();

        [BenchmarkCategory("Depth4"), Benchmark]
        public int ZLinq_Depth4() =>
            _array
            .AsValueEnumerable()
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Sum();

        [BenchmarkCategory("Depth16"), Benchmark]
        public int ZLinq_Depth16() =>
            _array
            .AsValueEnumerable()
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
        public int ZLinq_Depth32() =>
            _array
            .AsValueEnumerable()
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
        public int ZLinq_Concat() =>
            _array
            .AsValueEnumerable()
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
                .AsValueEnumerable()
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
