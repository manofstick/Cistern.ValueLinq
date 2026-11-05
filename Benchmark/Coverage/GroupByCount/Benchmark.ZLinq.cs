using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class GroupByCount
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .GroupBy(x => x % 100)
            .Count();
    }
}
