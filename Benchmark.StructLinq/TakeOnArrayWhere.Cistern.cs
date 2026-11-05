using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class TakeOnArrayWhere
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array
                .Where(x => true)
                .Take(TakeCount))
            {
                sum += i;
            }

            return sum;
        }
    }
}
