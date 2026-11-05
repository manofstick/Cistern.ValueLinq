using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ArrayWhereCount
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array
            .Where(x => (x & 1) == 0)
            .Count();
    }
}
