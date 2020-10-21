using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.SelectSelectToList
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public List<int> LinqAF() => _double.Select(x => (float)x).Select(x => (int)x).ToList();
    }
#endif
}
