using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectWhereSum
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .Select(x => x * 2)
            .Where(y => y % 3 == 0)
            .Sum();
    }
}
