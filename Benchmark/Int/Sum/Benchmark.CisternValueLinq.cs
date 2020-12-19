using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Int
{
    partial class Sum
    {
        [Benchmark]
        public double CisternValueLinq() => _data.Sum();
        [Benchmark]
        public double CisternValueLinq_SIMDOptions_Fastest() => _data.Sum(ValueLinq.Maths.SIMDOptions.Fastest);
        [Benchmark]
        public double CisternValueLinq_SIMDOptions_WithOverflowHandling() => _data.Sum(ValueLinq.Maths.SIMDOptions.WithOverflowHandling);

        [Benchmark]
        public double CisternValueLinq_Foreach()
        {
            var total = 0.0;
            foreach (var item in _data.OfEnumerable())
                total += item;
            return total;
        }
    }
}
