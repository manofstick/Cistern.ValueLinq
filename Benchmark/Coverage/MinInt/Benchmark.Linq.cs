using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class MinInt
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Min();
    }
}
