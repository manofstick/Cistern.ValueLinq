using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToLookupCount
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .ToLookup(x => x % 100)
            .Count;
    }
}
