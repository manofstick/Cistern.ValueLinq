using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class EnumeratorsOfClassComparison
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var i in array.OfArray())
            {
                sum += i.Element;
            }

            return sum;
        }
    }
}
