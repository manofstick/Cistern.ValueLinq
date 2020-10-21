using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.WhereToList
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public List<double> LinqAF() => _double.Where(x => x > 0).ToList();
    }
#endif
}
