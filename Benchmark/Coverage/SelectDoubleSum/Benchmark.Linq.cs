using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectDoubleSum
    {
        [Benchmark]
        public double Linq() =>
            _data
            .Select(x => x * 0.5)
            .Sum();
    }
}
