using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Max
    {
        [Benchmark]
        public int CisternValueLinq() => Enumerable
            .Range(0, Count)
            .Max();
    }
}
