using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class SelectSum
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
