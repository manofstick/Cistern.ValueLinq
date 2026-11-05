using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ArrayWhereSelectSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array
            .Where(x => (x & 1) == 0)
            .Select(x => x * 2)
            .Sum();
    }
}
