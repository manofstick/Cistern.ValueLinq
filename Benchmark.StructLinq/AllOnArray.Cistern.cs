using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class AllOnArray
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            array
            .All(x => x < Count / 2);
    }
}
