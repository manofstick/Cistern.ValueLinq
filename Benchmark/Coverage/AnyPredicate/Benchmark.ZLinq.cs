using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AnyPredicate
    {
        [Benchmark]
        public bool ZLinq() =>
            _data
            .AsValueEnumerable()
            .Any(x => x < 0);
    }
}
