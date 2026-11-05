using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AggregateSum
    {
        [Benchmark]
        public long ZLinq() =>
            _data
            .AsValueEnumerable()
            .Aggregate(0L, (acc, x) => acc + x);
    }
}
