using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectWhereSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Select(x => x * 2)
            .Where(y => y % 3 == 0)
            .Sum();
    }
}
