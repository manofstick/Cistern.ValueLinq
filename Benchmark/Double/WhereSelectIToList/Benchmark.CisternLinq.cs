using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class WhereSelectIToList
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.Where(x => x > 0).Select((x,i) => x + i).ToList();
    }
#endif
}
