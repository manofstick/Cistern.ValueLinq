using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class SelectWhereToList
    {
        [Benchmark(Baseline = true)]
        public List<double> Linq() => _double.Select(x => x + 1).Where(x => x > 20).ToList();
    }
}
