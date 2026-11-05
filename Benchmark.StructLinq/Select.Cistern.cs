using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Select
    {
        [Benchmark]
        public double CisternValueLinq() => Enumerable
            .Range(0, Count)
            .Select(x => x * 2.0)
            .Sum();
    }
}
