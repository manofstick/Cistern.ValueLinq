using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class SumOnList
    {
        [Benchmark]
        public int CisternValueLinq() =>
            list.Sum();
    }
}
