using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class Sum
    {
        [Benchmark]
        public double CisternLinq() => _double.Sum();
    }
#endif
}
