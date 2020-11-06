using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class ToList
    {
        [Benchmark]
        public List<double> LinqAF() => _double.ToList();
    }
#endif
}
