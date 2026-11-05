using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ElementAtOnArray
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array
            .ElementAt(Count / 2);

        [Benchmark]
        public int CisternValueLinqEnumerable() =>
            enumerable
            .ElementAt(Count / 2);
    }
}
