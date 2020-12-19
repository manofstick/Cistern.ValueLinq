using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Float
{
#if LINQAF
    partial class Sum
    {
        [Benchmark]
        public double LinqAF() => _data.Sum();
    }
#endif
}
