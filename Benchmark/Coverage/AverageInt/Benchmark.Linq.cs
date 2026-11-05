using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AverageInt
    {
        [Benchmark]
        public double Linq() =>
            _data
            .Average();
    }
}
