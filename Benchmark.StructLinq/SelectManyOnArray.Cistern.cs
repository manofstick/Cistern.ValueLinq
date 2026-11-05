using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class SelectManyOnArray
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array.SelectMany(x => x))
            {
                sum += i;
            }

            return sum;
        }

        [Benchmark]
        public int CisternValueLinqWhereReturnIsValueEnumerable()
        {
            var sum = 0;
            foreach (var i in array.OfArray().SelectMany(x => x.OfArray()))
            {
                sum += i;
            }

            return sum;
        }
    }
}
