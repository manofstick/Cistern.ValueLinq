using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.SelectToList
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public List<double> LinqAF() => _double.Select(x => x * 2).ToList();
    }
#endif
}
