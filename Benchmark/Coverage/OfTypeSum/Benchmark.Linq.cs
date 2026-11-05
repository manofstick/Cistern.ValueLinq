using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class OfTypeSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .OfType<int>()
            .Sum();
    }
}
