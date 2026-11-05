using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectSelectSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Select(x => x + 1)
            .Select(x => x * 2)
            .Sum();
    }
}
