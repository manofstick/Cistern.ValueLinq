using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class SelectWhereToList
    {
        [Benchmark]
        public List<double> LinqAF() => _double.Select(x => x + 1).Where(x => x > 20).ToList();
    }
#endif
}
