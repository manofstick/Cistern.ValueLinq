using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.WhereSelectToList
{
    partial class Benchmark
    {
        [Benchmark]
        public List<double> LinqAF() => _double.Where(x => x > 0).Select(x => x + 1).ToList();
    }
}
