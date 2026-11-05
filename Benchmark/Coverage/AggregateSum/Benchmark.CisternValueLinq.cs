using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AggregateSum
    {
        [Benchmark]
        public long CisternValueLinq() =>
            _data
            .Aggregate(0L, (acc, x) => acc + x);
    }
}
