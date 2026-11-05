using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Except
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array1.Except(array2))
            {
                sum += i;
            }

            return sum;
        }

        [Benchmark]
        public int CisternValueLinqSum() =>
            array1
            .Except(array2)
            .Sum();
    }
}
