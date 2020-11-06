using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class ToList
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.ToList();
    }
#endif
}
