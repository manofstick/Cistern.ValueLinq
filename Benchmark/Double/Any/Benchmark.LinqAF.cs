using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double.Any
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public bool LinqAF() => _double.Any();
    }
#endif
}
