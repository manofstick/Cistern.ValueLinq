using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Int
{
    partial class Average
    {
        [Benchmark(Baseline = true)]
        public double Linq() => _data.Average();

        [Benchmark]
        public double Linq_Foreach()
        {
            var count = 0;
            var total = 0;
            foreach (var item in _data)
            {
                ++count;
                total += item;
            }
            return (double)total/count;
        }
    }
}
