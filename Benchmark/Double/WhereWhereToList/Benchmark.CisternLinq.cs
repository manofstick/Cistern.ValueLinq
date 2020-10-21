using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.WhereWhereToList
{
#if CISTERNLINQ
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.Where(x => ((int)x % 11) != 0).Where(x => ((int)x % 13) != 0).ToList();
    }
#endif
}
