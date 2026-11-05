using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToLookupCount
    {
        [Benchmark]
        public int Linq() =>
            _data
            .ToLookup(x => x % 100)
            .Count;
    }
}
