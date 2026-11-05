using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ArrayOfIntSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array
            .Sum();

        [Benchmark]
        public int CisternValueLinqEnumerable() =>
            sysArray
            .Sum();
    }
}
