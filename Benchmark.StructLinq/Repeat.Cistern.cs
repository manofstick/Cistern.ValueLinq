using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Repeat
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            int sum = 0;
            foreach (var result in Enumerable.Repeat(value, Count))
            {
                sum += result;
            }

            return sum;
        }
    }
}
