using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.ToList
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public List<double> LinqAF() => _double.ToList();
    }
#endif
}
