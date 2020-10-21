using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.ToList
{
#if CISTERNLINQ
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.ToList();
    }
#endif
}
