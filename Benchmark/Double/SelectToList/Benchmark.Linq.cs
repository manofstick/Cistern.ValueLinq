using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double.SelectToList
{
    partial class Benchmark
    {
        [Benchmark(Baseline = true)]
        public List<double> Linq() => _double.Select(x => x * 2).ToList();
    }
}
