using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double.ToList
{
    partial class Benchmark
    {
        [Benchmark(Baseline = true)]
        public List<double> Linq() => _double.ToList();
    }
}
