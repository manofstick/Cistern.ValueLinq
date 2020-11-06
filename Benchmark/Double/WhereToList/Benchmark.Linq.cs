using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class WhereToList
    {
        [Benchmark(Baseline = true)]
        public List<double> Linq() => _double.Where(x => x > 0).ToList();
    }
}
