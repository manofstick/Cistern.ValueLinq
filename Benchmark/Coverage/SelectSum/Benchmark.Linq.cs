using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Select(x => x * 2)
            .Sum();
    }
}
