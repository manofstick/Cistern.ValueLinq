using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class EnumeratorsComparison
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array.OfArray())
            {
                sum += i;
            }

            return sum;
        }
    }
}
