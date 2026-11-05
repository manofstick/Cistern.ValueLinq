using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class MinInt
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .Min();
    }
}
