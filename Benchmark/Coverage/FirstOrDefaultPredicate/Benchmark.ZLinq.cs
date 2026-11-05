using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class FirstOrDefaultPredicate
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .FirstOrDefault(x => x < 0);
    }
}
