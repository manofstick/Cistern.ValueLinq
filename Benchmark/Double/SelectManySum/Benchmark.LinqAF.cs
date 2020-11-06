using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class SelectManySum
    {
        [Benchmark]
        public double LinqAF() => _double.SelectMany(x => _double).Sum();
    }
#endif
}
