using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Where(x => x % 3 == 0)
            .Sum();
    }
}
