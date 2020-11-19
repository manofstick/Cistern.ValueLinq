using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class Max
    {
        [Benchmark(Baseline = true)]
        public double Linq() => _double.Max();

        [Benchmark]
        public double Handcoded()
        {
            var max = double.MinValue;
            foreach (var item in _double)
                if (item > max)
                    max = item;
            return max;
        }
    }
}
