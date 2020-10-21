using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.SelectWhereToList
{
#if CISTERNLINQ
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.Select(x => x + 1).Where(x => x > 0).ToList();
    }
#endif
}
