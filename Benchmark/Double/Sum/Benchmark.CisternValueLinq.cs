using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Double
{
    partial class Sum
    {
        [Benchmark]
        public double CisternValueLinq() => _double.Sum();

        [Benchmark]
        public double CisternValueLinq_Foreach()
        {
            var total = 0.0;
            foreach (var item in _double.OfEnumerable())
                total += item;
            return total;
        }
    }
}
