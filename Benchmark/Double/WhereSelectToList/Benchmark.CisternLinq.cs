using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.WhereSelectToList
{
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.Where(x => x > 0).Select(x => x + 1).ToList();
    }
}
