using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class DistinctOnBigStruct
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            var structEqualityComparer = new StructEqualityComparer();
            foreach (var i in array.Distinct(structEqualityComparer))
            {
                sum += i.Element;
            }

            return sum;
        }
    }
}
