using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double.Sum
{
#if CISTERNLINQ
    partial class Benchmark
    {
        [Benchmark]
        public double CisternLinq() => _double.Sum();
    }
#endif
}
