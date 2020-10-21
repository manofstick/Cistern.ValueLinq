using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double.Sum
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public double LinqAF() => _double.Sum();
    }
#endif
}
