using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Distinct
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array.Distinct())
            {
                sum += i;
            }

            return sum;
        }

        [Benchmark]
        public int CisternValueLinqSum() =>
            array
            .Distinct()
            .Sum();
    }
}
