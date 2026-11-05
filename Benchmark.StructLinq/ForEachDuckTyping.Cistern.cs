using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ForEachDuckTyping
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

        [Benchmark]
        public int CisternValueLinqAsIEnumerable()
        {
            var sum = 0;
            foreach (var i in array.OfArray().AsEnumerable())
            {
                sum += i;
            }
            return sum;
        }
    }
}
