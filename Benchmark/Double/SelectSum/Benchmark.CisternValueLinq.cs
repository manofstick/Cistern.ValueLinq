using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Double
{
    partial class SelectSum
    {
        [Benchmark]
        public double CisternValueLinq() => _double.Select(x => x * 2).Sum();

        [Benchmark]
        public double CisternValueLinq_Foreach()
        {
            var total = 0.0;
            foreach (var item in _double.Select(x => x * 2))
                total += item;
            return total;
        }
    }
}
