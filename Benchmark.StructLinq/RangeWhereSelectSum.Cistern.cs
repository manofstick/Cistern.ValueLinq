using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class RangeWhereSelectSum
    {
        [Benchmark]
        public int CisternValueLinq() => Enumerable
            .Range(0, Count)
            .Where(x => (x & 1) == 0)
            .Select(x => x * 2)
            .Sum();
    }
}
