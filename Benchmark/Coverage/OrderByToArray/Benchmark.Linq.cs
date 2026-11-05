using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class OrderByToArray
    {
        [Benchmark]
        public int[] Linq() =>
            _data
            .OrderBy(x => x)
            .ToArray();
    }
}
