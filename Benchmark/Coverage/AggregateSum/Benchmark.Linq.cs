using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AggregateSum
    {
        [Benchmark]
        public long Linq() =>
            _data
            .Aggregate(0L, (acc, x) => acc + x);
    }
}
