using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class ToList
    {
        [Benchmark(Baseline = true)]
        public List<double> Linq() => _double.ToList();
    }
}
