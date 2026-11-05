using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ReverseToArray
    {
        [Benchmark]
        public int[] Linq() =>
            _data
            .Reverse()
            .ToArray();
    }
}
