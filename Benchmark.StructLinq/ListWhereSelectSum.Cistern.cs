using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ListWhereSelectSum
    {
        [Benchmark]
        public int CisternValueLinq() => list
            .Where(x => (x & 1) == 0)
            .Select(x => x * 2)
            .Sum();
    }
}
