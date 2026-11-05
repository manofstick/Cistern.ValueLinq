using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ForEachOnListOfString
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in list.OfList())
            {
                sum += i.Length;
            }
            return sum;
        }
    }
}
