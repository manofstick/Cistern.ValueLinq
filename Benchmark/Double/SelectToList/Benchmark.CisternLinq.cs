using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class SelectToList
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.Select(x => x*2).ToList();
    }
#endif
}
