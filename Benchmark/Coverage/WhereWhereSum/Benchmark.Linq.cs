using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereWhereSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Where(x => x % 2 == 0)
            .Where(x => x % 3 == 0)
            .Sum();
    }
}
