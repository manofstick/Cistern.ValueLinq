using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class GroupByCount
    {
        [Benchmark]
        public int Linq() =>
            _data
            .GroupBy(x => x % 100)
            .Count();
    }
}
