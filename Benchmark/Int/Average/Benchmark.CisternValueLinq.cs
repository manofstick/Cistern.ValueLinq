using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Int
{
    partial class Average
    {
        [Benchmark]
        public double CisternValueLinq() => _data.Average();
        [Benchmark]
        public double CisternValueLinq_SIMDOptions_Fastest() => _data.Average(ValueLinq.Maths.SIMDOptions.Fastest);
        [Benchmark]
        public double CisternValueLinq_SIMDOptions_WithOverflowHandling() => _data.Average(ValueLinq.Maths.SIMDOptions.WithOverflowHandling);

        [Benchmark]
        public double CisternValueLinq_Foreach()
        {
            var count = 0;
            var total = 0;
            foreach (var item in _data.OfEnumerable())
            {
                ++count;
                total += item;
            }
            return (double)total/count;
        }
    }
}
