using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double.Sum
{
    partial class Benchmark
    {
        [Benchmark]
        public double CisternLinq() => _double.Sum();
    }
}
