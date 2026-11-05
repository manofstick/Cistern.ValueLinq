using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ForEachOnArrayWhereSelect
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var enumerable = array
                             .Where(x => (x & 1) == 0)
                             .Select(x => x * 2);
            var sum = 0;
            foreach (var i in enumerable)
            {
                sum += i;
            }

            return sum;
        }

        [Benchmark]
        public int CisternValueLinqAsEnumerable()
        {
            var enumerable = array
                             .Where(x => (x & 1) == 0)
                             .Select(x => x * 2);

            var sum = 0;
            foreach (var i in enumerable.AsEnumerable())
            {
                sum += i;
            }

            return sum;
        }
    }
}
