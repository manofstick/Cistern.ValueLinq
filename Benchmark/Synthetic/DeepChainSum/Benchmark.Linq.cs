using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class DeepChainSum
    {
        [BenchmarkCategory("Depth1"), Benchmark]
        public int Linq_Depth1() =>
            _array
            .Select(x => x + 1)
            .Sum();

        [BenchmarkCategory("Depth2"), Benchmark]
        public int Linq_Depth2() =>
            _array
            .Select(x => x + 1).Where(x => x >= 0)
            .Sum();

        [BenchmarkCategory("Depth4"), Benchmark]
        public int Linq_Depth4() =>
            _array
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Sum();

        [BenchmarkCategory("Depth16"), Benchmark]
        public int Linq_Depth16() =>
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
        public int Linq_Depth32() =>
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
        public int Linq_Concat() =>
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
