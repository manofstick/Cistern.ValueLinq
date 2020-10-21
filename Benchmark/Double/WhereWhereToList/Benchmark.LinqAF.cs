using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.WhereWhereToList
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public List<double> LinqAF() => _double.Where(x => ((int)x % 11) != 0).Where(x => ((int)x % 13) != 0).ToList();
    }
#endif
}
