using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectIndexSum
    {
        [Benchmark]
        public long ZLinq() =>
            _data
            .AsValueEnumerable()
            .Select((x, i) => (long)x * i)
            .Sum();
    }
}
