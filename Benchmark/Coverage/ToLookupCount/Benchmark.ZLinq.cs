using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToLookupCount
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .ToLookup(x => x % 100)
            .Count;
    }
}
