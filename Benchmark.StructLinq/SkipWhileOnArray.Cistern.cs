using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class SkipWhileOnArray
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array.SkipWhile(x => x < 5000))
            {
                sum += i;
            }

            return sum;
        }
    }
}
