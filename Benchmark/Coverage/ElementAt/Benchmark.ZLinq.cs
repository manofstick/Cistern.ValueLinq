using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ElementAt
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .ElementAt(Length / 2);
    }
}
