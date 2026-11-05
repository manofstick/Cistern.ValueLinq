using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class WhereOnArrayOfClass
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array.Where(x => x.Element % 2 == 0))
            {
                sum += i.Element;
            }

            return sum;
        }
    }
}
