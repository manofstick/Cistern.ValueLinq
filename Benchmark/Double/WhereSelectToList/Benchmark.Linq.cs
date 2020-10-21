using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double.WhereSelectToList
{
    partial class Benchmark
    {
        [Benchmark(Baseline = true)]
        public List<double> Linq() => _double.Where(x => x > 0).Select(x => x + 1).ToList();
    }
}
