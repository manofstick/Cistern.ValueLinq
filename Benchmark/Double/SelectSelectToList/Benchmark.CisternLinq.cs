using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.SelectSelectToList
{
#if CISTERNLINQ
    partial class Benchmark
    {
        [Benchmark]
        public List<int> CisternLinq() => _double.Select(x => (float)x).Select(x => (int)x).ToList();
    }
#endif
}
