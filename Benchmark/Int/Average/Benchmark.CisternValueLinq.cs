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
            checked
            {
                var count = 0L;
                var total = 0L;
                foreach (var item in _data.OfEnumerable())
                {
                    ++count;
                    total += item;
                }
                return (double)total / count;
            }
        }
    }
}
