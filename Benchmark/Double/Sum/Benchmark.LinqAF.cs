using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class Sum
    {
        [Benchmark]
        public double LinqAF() => _double.Sum();
    }
#endif
}
