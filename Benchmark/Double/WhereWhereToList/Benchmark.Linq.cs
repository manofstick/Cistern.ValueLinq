using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class WhereWhereToList
    {
        [Benchmark(Baseline = true)]
        public List<double> Linq() => _double.Where(x => ((int)x % 11) != 0).Where(x => ((int)x % 13) != 0).ToList();
    }
}
