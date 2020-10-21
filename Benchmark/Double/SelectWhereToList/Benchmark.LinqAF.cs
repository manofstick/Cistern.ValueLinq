using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.SelectWhereToList
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public List<double> LinqAF() => _double.Select(x => x + 1).Where(x => x > 0).ToList();
    }
#endif
}
