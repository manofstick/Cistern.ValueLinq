using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class TakeWhileOnArray
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array.TakeWhile(x => x > 5000))
            {
                sum += i;
            }

            return sum;
        }
    }
}
