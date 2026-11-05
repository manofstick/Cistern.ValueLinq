using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToArray
    {
        [Benchmark]
        public int[] Linq() =>
            _data
            .ToArray();
    }
}
