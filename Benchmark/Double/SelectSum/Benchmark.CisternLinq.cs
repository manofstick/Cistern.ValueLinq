using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double.SelectSum
{
#if CISTERNLINQ
    partial class Benchmark
    {
        [Benchmark]
        public double CisternLinq() => _double.Select(x => x * 2).Sum();
    }
#endif
}
