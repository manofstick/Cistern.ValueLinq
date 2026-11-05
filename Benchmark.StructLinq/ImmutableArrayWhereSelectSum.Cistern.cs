using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ImmutableArrayWhereSelectSum
    {
        [Benchmark]
        public int CisternValueLinq() => array
            .OfImmutableArray()
            .Where(x => (x & 1) == 0)
            .Select(x => x * 2)
            .Sum();
    }
}
