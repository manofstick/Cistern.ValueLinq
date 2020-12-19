using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Int
{
#if LINQAF
    partial class Average
    {
        [Benchmark]
        public double LinqAF() => _data.Average();
    }
#endif
}
