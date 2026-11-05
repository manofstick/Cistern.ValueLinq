using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class OfTypeSum
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .OfType<int>()
            .Sum();
    }
}
