using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereSelectSum
    {
        [Benchmark]
        public long ZLinq() =>
            _data
            .AsValueEnumerable()
            .Where(x => x % 3 == 0)
            .Select(x => (long)x * x)
            .Sum();
    }
}
