using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Double.SelectManySum
{
    partial class Benchmark
    {
        [Benchmark(Baseline = true)]
        public double Linq() => _double.SelectMany(x => _double).Sum();
    }
}
