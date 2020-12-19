using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Float
{
    partial class Sum
    {
        [Benchmark(Baseline = true)]
        public double Linq() => _data.Sum();

        [Benchmark]
        public double Linq_Foreach()
        {
            var total = 0.0;
            foreach (var item in _data)
                total += item;
            return total;
        }
    }
}
