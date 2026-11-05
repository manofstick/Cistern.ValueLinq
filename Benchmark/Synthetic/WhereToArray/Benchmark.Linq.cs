using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class WhereToArray
    {
        [Benchmark]
        public int[] Linq() =>
            _data
            .Where(x => x % 2 == 0)
            .ToArray();
    }
}
