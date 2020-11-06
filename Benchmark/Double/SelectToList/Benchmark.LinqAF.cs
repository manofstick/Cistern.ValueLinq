using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class SelectToList
    {
        [Benchmark]
        public List<double> LinqAF() => _double.Select(x => x * 2).ToList();
    }
#endif
}
