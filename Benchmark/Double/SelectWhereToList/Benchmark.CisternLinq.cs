using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class SelectWhereToList
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.Select(x => x + 1).Where(x => x > 20).ToList();
    }
#endif
}
