using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double.SelectManySum
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public double LinqAF() => _double.SelectMany(x => _double).Sum();
    }
#endif
}
