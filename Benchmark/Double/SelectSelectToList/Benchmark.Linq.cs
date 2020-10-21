using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double.SelectSelectToList
{
    partial class Benchmark
    {
        [Benchmark(Baseline = true)]
        public List<int> Linq() => _double.Select(x => (float)x).Select(x => (int)x).ToList();
    }
}
