using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ArraySelectCount
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array
            .Select(x => x * 2)
            .Count();
    }
}
