using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectSelectSum
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .Select(x => x + 1)
            .Select(x => x * 2)
            .Sum();
    }
}
