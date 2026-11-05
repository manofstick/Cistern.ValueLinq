using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ForEachOnListWithSelect
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in list.Select(x => x * 2))
            {
                sum += i;
            }

            return sum;
        }

        [Benchmark]
        public int CisternValueLinqAsEnumerable()
        {
            var sum = 0;
            foreach (var i in list.Select(x => x * 2).AsEnumerable())
            {
                sum += i;
            }
            return sum;
        }
    }
}
