using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class WhereToList
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.Where(x => x > 0).ToList();
    }
#endif
}
