using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class SumOnSelectMany
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array
            .SelectMany(x => x)
            .Sum();
    }
}
