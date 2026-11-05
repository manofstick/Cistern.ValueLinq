using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereIndexSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Where((x, i) => i % 2 == 0)
            .Sum();
    }
}
