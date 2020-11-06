using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class SelectManySum
    {
        [Benchmark(Baseline = true)]
        public double Linq() => _double.SelectMany(x => _double).Sum();
    }
}
