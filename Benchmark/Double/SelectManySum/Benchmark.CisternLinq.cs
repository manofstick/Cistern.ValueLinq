using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class SelectManySum
    {
        [Benchmark]
        public double CisternLinq() => _double.SelectMany(x => _double).Sum();
    }
#endif
}
