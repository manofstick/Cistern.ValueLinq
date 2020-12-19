using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
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
            checked
            {
                var count = 0L;
                var total = 0L;
                foreach (var item in _data)
                {
                    ++count;
                    total += item;
                }
                return (double)total / count;
            }
        }
    }
}
