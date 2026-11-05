using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class FirstOnArray
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array
            .First();

        [Benchmark]
        public int CisternValueLinqEnumerable() =>
            enumerable
            .First();
    }
}
