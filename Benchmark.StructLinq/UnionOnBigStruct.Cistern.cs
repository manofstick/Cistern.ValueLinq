using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class UnionOnBigStruct
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array1.Union(array2))
            {
                sum += i.Element;
            }

            return sum;
        }
    }
}
