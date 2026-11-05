using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectDoubleSum
    {
        [Benchmark]
        public double ZLinq() =>
            _data
            .AsValueEnumerable()
            .Select(x => x * 0.5)
            .Sum();
    }
}
