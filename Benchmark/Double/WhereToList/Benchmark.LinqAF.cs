using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class WhereToList
    {
        [Benchmark]
        public List<double> LinqAF() => _double.Where(x => x > 0).ToList();
    }
#endif
}
