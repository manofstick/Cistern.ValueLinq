using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereWhereSum
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .Where(x => x % 2 == 0)
            .Where(x => x % 3 == 0)
            .Sum();
    }
}
