using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Int
{
#if LINQAF
    partial class Sum
    {
        [Benchmark]
        public double LinqAF() => _data.Sum();
    }
#endif
}
