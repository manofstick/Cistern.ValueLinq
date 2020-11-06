using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class SelectSum
    {
        [Benchmark(Baseline = true)]
        public double Linq() => _double.Select(x => x * 2).Sum();

        [Benchmark]
        public double Linq_Foreach()
        {
            var total = 0.0;
            foreach (var item in _double.Select(x => x * 2))
                total += item;
            return total;
        }
    }
}
