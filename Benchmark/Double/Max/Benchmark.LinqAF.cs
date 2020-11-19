using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class Max
    {
        [Benchmark]
        public double LinqAF() => _double.Max();
    }
#endif
}
