using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Double.Sum
{
    partial class Benchmark
    {
        [Benchmark(Baseline = true)]
        public double Linq() => _double.Sum();
    }
}
