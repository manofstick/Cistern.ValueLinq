using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AllPredicate
    {
        [Benchmark]
        public bool ZLinq() =>
            _data
            .AsValueEnumerable()
            .All(x => x >= 0);
    }
}
