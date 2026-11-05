using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class SelectOnArrayOfClass
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array.Select(x => x.Element))
            {
                sum += i;
            }

            return sum;
        }
    }
}
