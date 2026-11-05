using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Sum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            Enumerable.Range(0, Count).Sum();

        [Benchmark]
        public int CisternValueLinqSelector() =>
            Enumerable.Range(0, Count).Sum(x => x);

        [Benchmark]
        public int CisternValueLinqForEach()
        {
            var sum = 0;
            foreach (var i in Enumerable.Range(0, Count))
            {
                sum += i;
            }

            return sum;
        }
    }
}
