using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class OrderByArrayOfInt
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array.OrderBy(x => x))
            {
                sum += i;
            }

            return sum;
        }
    }
}
