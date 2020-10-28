using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double.SelectManySum
{
#if CISTERNLINQ
    partial class Benchmark
    {
        [Benchmark]
        public double CisternLinq() => _double.SelectMany(x => _double).Sum();
    }
#endif
}
