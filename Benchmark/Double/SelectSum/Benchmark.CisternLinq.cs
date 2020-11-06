using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class SelectSum
    {
        [Benchmark]
        public double CisternLinq() => _double.Select(x => x * 2).Sum();
    }
#endif
}
