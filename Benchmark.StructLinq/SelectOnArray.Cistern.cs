using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class SelectOnArray
    {
        [Benchmark]
        public double CisternValueLinq()
        {
            var sum = 0.0;
            foreach (var i in array.Select(x => x * 2.0))
            {
                sum += i;
            }

            return sum;
        }
    }
}
