using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double.SelectSum
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public double LinqAF() => _double.Select(x => x * 2).Sum();

        [Benchmark]
        public double LinqAF_Foreach()
        {
            var total = 0.0;
            foreach (var item in _double.Select(x => x * 2))
                total += item;
            return total;
        }
    }
#endif
}
